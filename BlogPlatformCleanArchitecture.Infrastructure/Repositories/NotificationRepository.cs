using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class NotificationRepository(ApplicationDbContext _context) : INotificationRepository
    {
        public async Task AddANotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddNotificationsAsync(List<Notification> notifications)
        {

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId, string type, int relatedPostId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.Type == type && n.RelatedPostId == relatedPostId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteNotificationsAsync(List<Notification> notifications)
        {
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task MarkNotificationAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}
