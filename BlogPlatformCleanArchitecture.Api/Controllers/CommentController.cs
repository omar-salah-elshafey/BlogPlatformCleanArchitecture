using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Services;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using System.Security.Claims;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        public readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor httpContextAccessor)
        {
            _commentService = commentService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpPost("add-comment")]
        public async Task<IActionResult> AddCommentAsync(CommentDto commentDto)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userName = userClaims!.Identity?.Name;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _commentService.CreateCommentAsync(commentDto, userId, userName);
            return Ok(result);
        }

        [HttpGet("get-comments-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCommentsCountAsync()
        {
            var commentsCount = await _commentService.GetCommentsCountAsync();
            return Ok(commentsCount);
        }

        [HttpGet("get-all-comments")]
        public async Task<IActionResult> GetAllCommentsAsync(int pageNumber = 1, int pageSize = 5)
        {
            var paginatedComments = await _commentService.GetAllCommentsAsync(pageNumber, pageSize);
            return Ok(paginatedComments);
        }

        [HttpGet("get-comments-by-user")]
        public async Task<IActionResult> GetCommentsByUserAsync(string UserName, int pageNumber = 1, int pageSize = 5)
        {
            var paginatedComments = await _commentService.GetCommentsByUserAsync(UserName, pageNumber, pageSize);
            return Ok(paginatedComments);
        }

        [HttpGet("get-comments-by-post")]
        public async Task<IActionResult> GetCommentsByPostAsync(int postId, int pageNumber = 1, int pageSize = 5)
        {
            var paginatedComments = await _commentService.GetCommentsByPostAsync(postId, pageNumber, pageSize);
            return Ok(paginatedComments);
        }

        [Authorize]
        [HttpPut("update-comment")]
        public async Task<IActionResult> UpdateCommentAsync(int id, CommentDto commentDto)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userName = userClaims!.Identity?.Name;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedComment = await _commentService.UpdateCommentAsync(id, commentDto, userId, userName);
            return Ok(updatedComment);
        }

        [Authorize]
        [HttpDelete("delete-comment")]
        public async Task<IActionResult> DeleteCommentAsync(int id)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            await _commentService.DeleteCommentAsync(id, userId);
            return Ok(new {message = "Deleted Successfully" });
        }
    }
}
