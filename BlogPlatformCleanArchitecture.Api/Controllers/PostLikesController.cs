using BlogPlatformCleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostLikesController : ControllerBase
    {
        private readonly IPostLikeService _postLikeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostLikesController(IPostLikeService postLikeService, IHttpContextAccessor httpContextAccessor)
        {
            _postLikeService = postLikeService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("{postId}/toggle-like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var likes = await _postLikeService.ToggleLikeAsync(postId, userId);
            return Ok(likes);
        }

        [HttpGet("{postId}/likes")]
        public async Task<IActionResult> GetPostLikes(int postId)
        {
            var likes = await _postLikeService.GetPostLikesAsync(postId);
            return Ok(likes);
        }
    }
}
