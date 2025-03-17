using BlogPlatformCleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BlogPlatformCleanArchitecture.Infrastructure.Hubs
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToAllAsync(string methodName, object arg)
        {
            await _hubContext.Clients.All.SendAsync(methodName, arg);
        }

        public async Task SendToUserAsync(string userId, string methodName, object arg)
        {
            await _hubContext.Clients.User(userId).SendAsync(methodName, arg);
        }
    }
}
