using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace BlogPlatformCleanArchitecture.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
            _logger.LogInformation("Hub constructor called!");
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("🚨 No authenticated user found!");
            }
            else
            {
                _logger.LogInformation($"✅ User {userId} connected with ConnectionId {Context.ConnectionId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
