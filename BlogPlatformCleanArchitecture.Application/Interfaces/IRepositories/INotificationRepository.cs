using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface INotificationRepository
    {
        Task AddNotificationsAsync(List<Notification> notifications);
        Task AddANotificationAsync(Notification notification);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId, string type, int relatedPostId);
        Task DeleteNotificationsAsync(List<Notification> notifications);
        Task DeleteNotificationAsync(Notification notification);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task MarkNotificationAsReadAsync(Notification notification);
    }
}
