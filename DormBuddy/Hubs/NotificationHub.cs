using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DormBuddy.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // This requires proper Claim configuration
            if (userId != null)
            {
                ConnectedUsers.TryAdd(Context.ConnectionId, userId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUsers.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public static IEnumerable<string> GetActiveUsers()
        {
            return ConnectedUsers.Values;
        }
    }
}
