using Microsoft.AspNetCore.SignalR;

namespace BlogPlatformCleanArchitecture.Infrastructure.DependencyInjection
{
    public class SubBasedUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("sub")?.Value;
        }
    }
}