using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyNewPostAsync(Post post);
        Task NotifyPostLikedAsync(Post post, ApplicationUser liker);
        Task NotifyPostCommentedAsync(Post post, Comment comment);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId);
        Task MarkNotificationAsReadAsync(int id);
    }
}
