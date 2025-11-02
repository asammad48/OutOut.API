using Microsoft.AspNetCore.SignalR;

namespace OutOut.Infrastructure.Services
{
    public class NotificationHub : Hub<INotificationHub>
    {
        public string GetConnectionId() => Context.ConnectionId;

        public async Task AddUserToGroup(string connectionId, string userId)
        {
            await Groups.AddToGroupAsync(connectionId, userId);
        }
    }
}
