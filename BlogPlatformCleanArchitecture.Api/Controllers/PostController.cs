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
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;
        public PostController(IPostService postService, UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _userManager = userManager;
        }

        [HttpPost("create-post")]
        [Authorize(Roles = "Author, Admin")]
        public async Task<IActionResult> CreatePostAsync(PostDto postDto)
        {
            var authId = Request.Cookies["userId"];
            var authUserName = Request.Cookies["UserName"];
            var createdPost = await _postService.CreatePostAsync(postDto, authId, authUserName);
            if (createdPost == null)
                return BadRequest("An error occurred!");
            return Ok(createdPost);
        }

        [HttpGet("get-all-posts")]
        public async Task<IActionResult> GetAllPostsAsync()
        {
            var posts = await _postService.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("get-posts-by-user")]
        public async Task<IActionResult> GetPostsByUserAsync(string UserName)
        {
            var posts = await _postService.GetPostsByUserAsync(UserName);
            return Ok(posts);
        }

        [HttpGet("get-post-by-id")]
        public async Task<IActionResult> GetPostByIdAsync(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            return Ok(post);
        }

        [HttpPut("update-post")]
        [Authorize(Roles = "Author, Admin")]
        public async Task<IActionResult> UpdatePostAsync(int id, PostDto postDto)
        {
            var authId = Request.Cookies["userId"];
            var userName = Request.Cookies["userName"];
            var UpdatedPost = await _postService.UpdatePostAsync(id, postDto, authId, userName);
            if (UpdatedPost == null) return BadRequest(string.Empty);
            return Ok(UpdatedPost);
        }

        [HttpDelete("delete-post")]
        [Authorize(Roles = "Author, Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = Request.Cookies["userId"];
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var deletedPost = await _postService.DeletePostAsync(id, userId, isAdmin);
            if (!deletedPost) return NotFound(string.Empty);
            return Ok(deletedPost);
        }
    }
}
