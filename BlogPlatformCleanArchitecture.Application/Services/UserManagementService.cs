using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class UserManagementService: IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserManagementService> _logger;
        public UserManagementService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IAuthService authService, IHttpContextAccessor httpContextAccessor, ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetUSersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                if (!user.IsDeleted)
                {
                    var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    userDtos.Add(new UserDto
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = role,
                    });
                }
            }
            return userDtos;
        }

        public async Task<UserDto> GetUserProfileAsync()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userName = userClaims!.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                throw new Exception("User ID claim not found.");
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted)
                throw new UserNotFoundException("User Not Found!");
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Role = role
            };
        }

        public async Task<List<UserDto>> SearchUsersAsync(string searchQuery)
        {
            var normalizedSearchQuery = searchQuery.ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
                throw new NullOrWhiteSpaceInputException("Search query cannot be empty.");

            var users = await _userManager.Users
                .Where(user => !user.IsDeleted && user.UserName!.ToLower().Contains(normalizedSearchQuery))
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                userDtos.Add(new UserDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = role,
                });
            }

            return userDtos;
        }


        public async Task<string> ChangeRoleAsync(ChangeUserRoleDto changeRoleDto)
        {
            var user = await _userManager.FindByNameAsync(changeRoleDto.UserName);
            if (user == null || user.IsDeleted)
                return ("Invalid UserName!");
            if (!await _roleManager.RoleExistsAsync(changeRoleDto.Role))
                return ("Invalid Role!");
            if (await _userManager.IsInRoleAsync(user, changeRoleDto.Role))
                return ("User Is already assigned to this role!");
            var result = await _userManager.AddToRoleAsync(user, changeRoleDto.Role);
            return $"User {changeRoleDto.UserName} has been assignd to Role {changeRoleDto.Role} Successfully :)";
        }

        public async Task<string> DeleteUserAsync(string UserName, string CurrentUserName, string refreshToken)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            var currentUser = await _userManager.FindByNameAsync(CurrentUserName);
            var role = (await _userManager.GetRolesAsync(currentUser)).First().ToUpper();
            if (user is null || user.IsDeleted)
                return $"User with UserName: {UserName} isn't found!";
            if (!CurrentUserName.Equals(UserName) && role != "ADMIN")
                return $"You Are Not Allowed to perform this action!";
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return $"An Error Occured while Deleting the user{UserName}" ;
            if (UserName == CurrentUserName)
                await _authService.LogoutAsync(refreshToken);
            return $"User with UserName: '{UserName}' has been Deleted successfully" ;
        }

        public async Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByNameAsync(updateUserDto.UserName);
            if (user is null || user.IsDeleted == true)
                return new UpdateUserResponseModel { Message = $"User with UserName: {updateUserDto.UserName} isn't found!" };
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                return new UpdateUserResponseModel { Message = $"Failed to update user: {errors}" };
            }
            return new UpdateUserResponseModel
            {
                UserName = updateUserDto.UserName,
                FirstName = updateUserDto.FirstName,
                LastName = updateUserDto.LastName,
                Message = "User has been updated Successfully."
            };
        }
    }
}
