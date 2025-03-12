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
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PostController(IPostService postService, UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor httpContextAccessor)
        {
            _postService = postService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("create-post")]
        [Authorize(Roles = "Writer, Admin, SuperAdmin")]
        public async Task<IActionResult> CreatePostAsync(PostDto postDto)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var authId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var authUserName = userClaims!.Identity?.Name;
            var createdPost = await _postService.CreatePostAsync(postDto, authId, authUserName);
            return Ok(createdPost);
        }

        [HttpGet("get-posts-count")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> GetPostsCountAsync()
        {
            var postsCount = await _postService.GetPostsCountAsync();
            return Ok(postsCount);
        }

        [HttpGet("get-all-posts")]
        public async Task<IActionResult> GetAllPostsAsync(int pageNumber = 1, int pageSize = 5)
        {
            var paginatedPosts = await _postService.GetAllPostsAsync(pageNumber, pageSize);
            return Ok(paginatedPosts );
        }

        [HttpGet("get-posts-by-user/{UserName}")]
        public async Task<IActionResult> GetPostsByUserAsync(string UserName, int pageNumber = 1, int pageSize = 5)
        {
            var paginatedPosts = await _postService.GetPostsByUserAsync(UserName, pageNumber, pageSize);
            return Ok(paginatedPosts );
        }

        [HttpGet("get-post-by-id/{id}")]
        public async Task<IActionResult> GetPostByIdAsync(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            return Ok(post);
        }

        [HttpPut("update-post/{id}")]
        [Authorize(Roles = "Writer, Admin, SuperAdmin")]
        public async Task<IActionResult> UpdatePostAsync(int id, UpdatePostDto postDto)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = userClaims!.Identity?.Name;
            var UpdatedPost = await _postService.UpdatePostAsync(id, postDto, userId, userName);
            return Ok(UpdatedPost);
        }

        [HttpDelete("delete-post/{id}")]
        [Authorize(Roles = "Writer, Admin, SuperAdmin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            await _postService.DeletePostAsync(id, userId);
            return Ok(new {message = "Post Deleted Successfully!"});
        }

        [HttpPost("share-post/{postId}")]
        [Authorize]
        public async Task<IActionResult> SharePostAsync(int postId)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = userClaims!.Identity?.Name;

            var sharedPost = await _postService.SharePostAsync(postId, userId, userName);
            return Ok(sharedPost);
        }

        [HttpGet("get-user-feed/{userName}")]
        public async Task<IActionResult> GetUserFeedAsync(string userName, int pageNumber = 1, int pageSize = 10)
        {
            var feed = await _postService.GetUserFeedAsync(userName, pageNumber, pageSize);
            return Ok(feed);
        }

        [HttpGet("get-home-feed")]
        public async Task<IActionResult> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10)
        {
            var feed = await _postService.GetHomeFeedAsync(pageNumber, pageSize);
            return Ok(feed);
        }
    }
}
