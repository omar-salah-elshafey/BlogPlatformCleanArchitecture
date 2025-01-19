using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UnauthorizedAccessException = BlogPlatformCleanArchitecture.Application.ExceptionHandling.UnauthorizedAccessException;

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

        public async Task DeleteUserAsync(string UserName, string refreshToken)
        {
            _logger.LogWarning(UserName);
            _logger.LogWarning(refreshToken);
            var user = await _userManager.FindByNameAsync(UserName);
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var CurrentUserName = userClaims!.Identity?.Name;
            var currentUser = await _userManager.FindByNameAsync(CurrentUserName);
            var role = (await _userManager.GetRolesAsync(currentUser)).First().ToUpper();
            if (user is null || user.IsDeleted)
                throw new UserNotFoundException("User Not Found!");
            if (!CurrentUserName.Equals(UserName) && role != "ADMIN")
                throw new UnauthorizedAccessException("You aren't Authorized to do this action!");
            _logger.LogWarning(CurrentUserName);
            _logger.LogWarning(currentUser.ToString());
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception($"An Error Occured while Deleting the user{UserName}");
            if (UserName == CurrentUserName)
                await _authService.LogoutAsync(refreshToken);
        }

        public async Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByNameAsync(updateUserDto.UserName);
            if (user is null || user.IsDeleted == true)
                throw new UserNotFoundException($"User with UserName: {updateUserDto.UserName} isn't found!");
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var currentUserName = userClaims!.Identity?.Name;
            var currentUser = await _userManager.FindByNameAsync(currentUserName!);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser!, "Admin");
            if (!currentUserName!.Equals(updateUserDto.UserName) && !isAdmin)
            {
                _logger.LogError("You aren't Authorized to do this Action!");
                throw new UnauthorizedAccessException("You aren't Authorized to do this Action!");
            }
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user: {errors}");
            }
            _logger.LogInformation("User has been updated Successfully.");
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
