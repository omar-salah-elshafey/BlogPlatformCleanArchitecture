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

        public async Task<int> GetUsersCountAsync()
        {
            return await _userManager.Users.CountAsync(u => !u.IsDeleted);
        }

        public async Task<PaginatedResponseModel<UserDto>> GetUSersAsync(int pageNumber, int pageSize)
        {
            var totalItems = await _userManager.Users.CountAsync(u => !u.IsDeleted);
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted).AsSplitQuery()
                .OrderByDescending(u => u.DateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
            return new PaginatedResponseModel<UserDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = userDtos
            };
        }

        public async Task<UserDto> GetUserProfileAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new NullOrWhiteSpaceInputException("UserName can't be null!");
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

        public async Task<PaginatedResponseModel<UserDto>> SearchUsersAsync(string searchQuery, int pageNumber, int pageSize)
        {
            var normalizedSearchQuery = searchQuery.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
                throw new NullOrWhiteSpaceInputException("Search query cannot be empty.");
            var totalItems = await _userManager.Users
                .CountAsync(u => !u.IsDeleted && u.UserName!.ToLower().Contains(normalizedSearchQuery));
            var users = await _userManager.Users
                .Where(user => !user.IsDeleted && user.UserName!.ToLower().Contains(normalizedSearchQuery))
                .OrderByDescending(u => u.DateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return new PaginatedResponseModel<UserDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = userDtos
            };
        }


        public async Task ChangeRoleAsync(ChangeUserRoleDto changeRoleDto)
        {
            var user = await _userManager.FindByNameAsync(changeRoleDto.UserName);
            if (user == null || user.IsDeleted)
                throw new UserNotFoundException("Invalid UserName, User Not Found!");
            if (!await _roleManager.RoleExistsAsync(changeRoleDto.Role.ToLower()))
                throw new UserNotFoundException("Invalid Role!");
            if (await _userManager.IsInRoleAsync(user, changeRoleDto.Role.ToLower()))
                throw new DuplicateUsernameException("User Is already assigned to this role!");
            var currentrole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            await _userManager.RemoveFromRoleAsync(user, currentrole);
            var result = await _userManager.AddToRoleAsync(user, changeRoleDto.Role.ToLower());
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
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
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
                throw new ForbiddenAccessException("You aren't Authenticated to do this Action!");
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
