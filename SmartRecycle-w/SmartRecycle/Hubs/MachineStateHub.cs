using Microsoft.AspNetCore.SignalR;

namespace SmartRecycle.Hubs
{
    /// <summary>
    /// Flutter app connects here to get real-time machine state:
    /// login/logout/points updates/detection status
    /// </summary>
    public class MachineStateHub : Hub
    {
        // Flutter calls this after connecting to subscribe to a specific machine
        public async Task SubscribeToMachine(string machineId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"machine_{machineId}");
        }

        public async Task UnsubscribeFromMachine(string machineId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"machine_{machineId}");
        }
    }
}
