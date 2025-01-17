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
        public UserManagementController(UserManager<ApplicationUser> userManager, 
            IUserManagementService userManagementService)
        {
            _userManager = userManager;
            _userManagementService = userManagementService;
        }

        [HttpGet("get-users")]
        [Authorize]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = await _userManagementService.GetUSersAsync();
            if (users.Count == 0)
                return NotFound("No users found!");
            return Ok(users);
        }

        [HttpGet("get-current-user-profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfileAsync()
        {
            var userProfile = await _userManagementService.GetUserProfileAsync();

            return Ok(userProfile);
        }

        [Authorize]
        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var users = await _userManagementService.SearchUsersAsync(query);
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
        public async Task<IActionResult> DeleteUserAsync(string UserName)
        {
            var CurrentUserName = Request.Cookies["UserName"];
            var refreshToken = Request.Cookies["refreshToken"];
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userManagementService.DeleteUserAsync(UserName, CurrentUserName, refreshToken);
            return Ok(result);
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
