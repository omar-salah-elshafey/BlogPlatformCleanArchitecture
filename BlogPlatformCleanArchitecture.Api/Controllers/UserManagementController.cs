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
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserManagementService _userManagementService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserManagementController(UserManager<ApplicationUser> userManager,
            IUserManagementService userManagementService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _userManagementService = userManagementService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("get-users-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersCountAsync()
        {
            var usersCount = await _userManagementService.GetUsersCountAsync();
            return Ok(usersCount);
        }

        [HttpGet("get-all-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userManagementService.GetUSersAsync(pageNumber, pageSize);
            return Ok(users);
        }

        [HttpGet("get-current-user-profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfileAsync()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userName = userClaims!.Identity?.Name;
            var userProfile = await _userManagementService.GetUserProfileAsync(userName);

            return Ok(userProfile);
        }

        [HttpGet("get-user-profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfileAsync(string userName)
        {
            var userProfile = await _userManagementService.GetUserProfileAsync(userName);

            return Ok(userProfile);
        }

        [Authorize]
        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userManagementService.SearchUsersAsync(query, pageNumber, pageSize);
            return Ok(users);
        }


        [HttpPut("change-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoleAsync(ChangeUserRoleDto changeUserRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userManagementService.ChangeRoleAsync(changeUserRoleDto);

            return Ok(result);
        }

        [HttpDelete("delete-user")]
        [Authorize]
        public async Task<IActionResult> DeleteUserAsync(string UserName, string? refreshToken)
        {
            if(refreshToken == null)
                refreshToken = Request.Cookies["refreshToken"];
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _userManagementService.DeleteUserAsync(UserName, refreshToken);
            return Ok(new {message= $"User with UserName: '{UserName}' has been Deleted successfully" });
        }

        [HttpPut("update-user")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userManagementService.UpdateUserAsync(updateUserDto);
            return Ok(result);
        }
    }
}
