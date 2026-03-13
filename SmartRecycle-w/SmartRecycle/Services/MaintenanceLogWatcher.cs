using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Hubs;
using SmartRecycle.Models;

namespace SmartRecycle.Services
{
    public class MaintenanceLogWatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MaintenanceHub> _hub;
        private readonly ILogger<MaintenanceLogWatcher> _logger;

        private int _lastSeenId = -1;
        private bool _initialized = false;

        public MaintenanceLogWatcher(
            IServiceScopeFactory scopeFactory,
            IHubContext<MaintenanceHub> hub,
            ILogger<MaintenanceLogWatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MaintenanceLogWatcher started.");
            await InitializeLastSeenId();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForNewEntries(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking for new maintenance log entries.");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task InitializeLastSeenId()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SmartRecycleContext>();
                _lastSeenId = await db.MaintenanceLogs.AnyAsync()
                    ? await db.MaintenanceLogs.MaxAsync(x => x.Id)
                    : 0;
                _initialized = true;
                _logger.LogInformation("Initialized. Last ID: {Id}", _lastSeenId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize.");
                _lastSeenId = 0;
                _initialized = true;
            }
        }

        private async Task CheckForNewEntries(CancellationToken ct)
        {
            if (!_initialized) return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SmartRecycleContext>();

            var newEntries = await db.MaintenanceLogs
                .Include(x => x.Machine)
                .Where(x => x.Id > _lastSeenId)
                .OrderBy(x => x.Id)
                .Select(x => new MaintenanceLogDto
                {
                    Id              = x.Id,
                    MachineId       = x.MachineId,
                    // ← غيّر x.Machine.Name لو اسم الـ property مختلف في Machines model بتاعك
                    MachineName     = x.Machine != null ? x.Machine.Location : "Machine #" + x.MachineId,
                    Command         = x.Command,
                    MaintenanceDate = x.MaintenanceDate
                })
                .ToListAsync(ct);

            if (newEntries.Any())
            {
                _lastSeenId = newEntries.Max(x => x.Id);
                _logger.LogInformation("Broadcasting {Count} new entries.", newEntries.Count);
                await _hub.Clients.All.SendAsync("NewMaintenanceEntries", newEntries, ct);
            }
        }
    }

    public class MaintenanceLogDto
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
    }
}
