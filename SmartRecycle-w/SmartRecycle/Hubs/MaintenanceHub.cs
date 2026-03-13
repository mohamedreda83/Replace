using Microsoft.AspNetCore.SignalR;

namespace SmartRecycle.Hubs
{
    public class MaintenanceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "Real-time connection established");
            await base.OnConnectedAsync();
        }
    }
}
