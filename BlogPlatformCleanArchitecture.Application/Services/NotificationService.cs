using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationSender _notificationSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            UserManager<ApplicationUser> userManager,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _notificationSender = notificationSender;
        }

        public async Task NotifyNewPostAsync(Post post)
        {
            var author = await _userManager.FindByIdAsync(post.AuthorId);
            var users = await _userManager.Users
                .Where(u => u.Id != post.AuthorId && !u.IsDeleted)
                .ToListAsync();
            var message = $"{author.UserName} has published a new post.";

            var notifications = users.Select(user => new Notification
            {
                UserId = user.Id,
                Message = message,
                Type = "NewPost",
                RelatedPostId = post.Id
            }).ToList();

            await _notificationRepository.AddNotificationsAsync(notifications);

            foreach (var notification in notifications)
            {
                await _notificationSender.SendToUserAsync(notification.UserId, "ReceiveNotification", new
                {
                    id = notification.Id,
                    message,
                    postId = post.Id,
                    type = "NewPost"
                });
            }
        }

        public async Task NotifyPostLikedAsync(Post post, ApplicationUser liker)
        {
            if (post.AuthorId == liker.Id) return;

            var existingNotifications = await _notificationRepository.GetUnreadNotificationsAsync(post.AuthorId, "PostLiked", post.Id);
            await _notificationRepository.DeleteNotificationsAsync(existingNotifications);

            var likeCount = post.Likes.Count();
            string message = likeCount > 1
                ? $"{liker.UserName} and others liked your post."
                : $"{liker.UserName} liked your post.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                Type = "PostLiked",
                RelatedPostId = post.Id
            };

            await _notificationRepository.AddANotificationAsync(notification);

            await _notificationSender.SendToUserAsync(post.AuthorId, "ReceiveNotification", new
            {
                id = notification.Id,
                message,
                postId = post.Id,
                type = "PostLiked"
            });
        }

        public async Task NotifyPostCommentedAsync(Post post, Comment comment)
        {
            if (post.AuthorId == comment.UserId) return;

            var existingNotifications = await _notificationRepository.GetUnreadNotificationsAsync(post.AuthorId, "PostCommented", post.Id);
            await _notificationRepository.DeleteNotificationsAsync(existingNotifications);

            var commentCount = post.Comments.Count();
            var commenter = await _userManager.FindByIdAsync(comment.UserId);
            var message = commentCount > 1
                ? $"{commenter.UserName} and others commented on your post."
                : $"{commenter.UserName} commented on your post.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                Type = "PostCommented",
                RelatedPostId = post.Id
            };

            await _notificationRepository.AddANotificationAsync(notification);

            await _notificationSender.SendToUserAsync(post.AuthorId, "ReceiveNotification", new
            {
                id = notification.Id,
                message,
                postId = post.Id,
                type = "PostCommented"
            });
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId);
            return  notifications.Select(n => new NotificationDto(
                n.Id,
                n.Message,
                n.Type,
                n.RelatedPostId
            )).ToList();
        }

        public async Task MarkNotificationAsReadAsync(int id)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification is null)
                throw new NotFoundException("Item not found");
            await _notificationRepository.MarkNotificationAsReadAsync(notification);
            await _notificationRepository.DeleteNotificationAsync(notification);
        }
    }
}
