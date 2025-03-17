using BlogPlatformCleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController(INotificationService _notificationService, IHttpContextAccessor _httpContextAccessor) : ControllerBase
    {
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotificationsAsync()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkNotificationAsReadAsync(int id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok();
        }
    }
}
