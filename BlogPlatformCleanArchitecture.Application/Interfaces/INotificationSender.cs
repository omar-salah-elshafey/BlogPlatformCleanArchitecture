namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface INotificationSender
    {
        Task SendToAllAsync(string methodName, object arg);
        Task SendToUserAsync(string userId, string methodName, object arg);
    }
}
