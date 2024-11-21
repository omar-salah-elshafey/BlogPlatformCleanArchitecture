using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        public readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("add-comment")]
        public async Task<IActionResult> AddCommentAsync(CommentDto commentDto)
        {
            var UserId = Request.Cookies["userId"];
            var UserName = Request.Cookies["UserName"];
            var result = await _commentService.CreateCommentAsync(commentDto, UserId, UserName);
            if (result == null)
                return BadRequest("An error occurred!");
            return Ok(result);
        }

        [HttpGet("get-all-comments")]
        public async Task<IActionResult> GetAllCommentsAsync()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return Ok(comments);
        }

        [HttpGet("get-comments-by-user")]
        public async Task<IActionResult> GetCommentsByUserAsync(string UserName)
        {
            var result = await _commentService.GetCommentsByUserAsync(UserName);
            if (result == null) return NotFound(string.Empty);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("update-comment")]
        public async Task<IActionResult> UpdateCommentAsync(int id, CommentDto commentDto)
        {
            var userId = Request.Cookies["userId"];
            var userName = Request.Cookies["userName"];
            var updatedComment = await _commentService.UpdateCommentAsync(id, commentDto, userId, userName);
            if (updatedComment == null) return BadRequest(string.Empty);
            return Ok(updatedComment);
        }

        [Authorize]
        [HttpDelete("delete-comment")]
        public async Task<IActionResult> DeleteCommentAsync(int id)
        {
            var userId = Request.Cookies["userId"];
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var result = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
            if (!result)
                return NotFound(string.Empty);
            return Ok("Deleted Successfully");
        }
    }
}
