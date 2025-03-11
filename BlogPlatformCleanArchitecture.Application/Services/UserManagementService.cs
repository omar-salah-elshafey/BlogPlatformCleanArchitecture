using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlogPlatformCleanArchitecture.Domain.Enums;

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
                throw new NotFoundException("User Not Found!");
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
            var targetUser = await _userManager.FindByNameAsync(changeRoleDto.UserName);
            if (targetUser == null || targetUser.IsDeleted)
                throw new NotFoundException("Invalid UserName, User Not Found!");

            if (!await _roleManager.RoleExistsAsync(changeRoleDto.Role))
                throw new NotFoundException("Invalid Role!");

            var userClaims = _httpContextAccessor.HttpContext?.User;
            var currentUserName = userClaims!.Identity?.Name;
            var currentUser = await _userManager.FindByNameAsync(currentUserName!);

            await AuthorizeRoleChangeAsync(currentUser!, targetUser, changeRoleDto, currentUserName!);

            if (await _userManager.IsInRoleAsync(targetUser, changeRoleDto.Role))
                throw new DuplicateValueException("User is already assigned to this role!");

            var currentRole = (await _userManager.GetRolesAsync(targetUser)).FirstOrDefault();
            if (!string.IsNullOrEmpty(currentRole)) await _userManager.RemoveFromRoleAsync(targetUser, currentRole);

            await _userManager.AddToRoleAsync(targetUser, changeRoleDto.Role);

            _logger.LogInformation($"User {targetUser.UserName}'s role changed to {changeRoleDto.Role} by {currentUserName}");
        }

        public async Task DeleteUserAsync(string userName, string? refreshToken, string? password)
        {
            var targetUser = await _userManager.FindByNameAsync(userName);
            if (targetUser is null || targetUser.IsDeleted)
                throw new NotFoundException("User Not Found!");

            var userClaims = _httpContextAccessor.HttpContext?.User;
            var currentUserName = userClaims!.Identity?.Name;
            var currentUser = await _userManager.FindByNameAsync(currentUserName!);

            await AuthorizeDeletionAsync(currentUser!, targetUser, userName, currentUserName!);

            bool isSelfDelete = currentUserName!.Equals(userName);
            if (isSelfDelete)
            {
                if (string.IsNullOrEmpty(password))
                    throw new InvalidInputsException("Password is required to delete your own account!");
                if (!await _userManager.CheckPasswordAsync(currentUser!, password))
                    throw new InvalidInputsException("Incorrect password provided!");
            }

            _logger.LogWarning($"{currentUserName} is deleting {userName}");
            targetUser.IsDeleted = true;
            var result = await _userManager.UpdateAsync(targetUser);

            if (!result.Succeeded)
                throw new Exception($"An error occurred while deleting the user {userName}");

            if (userName == currentUserName)
                await _authService.LogoutAsync(refreshToken!);
        }

        public async Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var targetUser = await _userManager.FindByNameAsync(updateUserDto.UserName);
            if (targetUser is null || targetUser.IsDeleted == true)
                throw new NotFoundException($"User with UserName: {updateUserDto.UserName} isn't found!");

            var userClaims = _httpContextAccessor.HttpContext.User;
            var currentUserName = userClaims!.Identity?.Name;
            var currentUser = await _userManager.FindByNameAsync(currentUserName!);

            await AuthorizeUpdateAsync(currentUser!, targetUser, updateUserDto.UserName, currentUserName!);

            targetUser.FirstName = updateUserDto.FirstName;
            targetUser.LastName = updateUserDto.LastName;
            var result = await _userManager.UpdateAsync(targetUser);
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

        private async Task AuthorizeRoleChangeAsync(ApplicationUser currentUser, ApplicationUser targetUser,
            ChangeUserRoleDto changeRoleDto, string currentUserName)
        {
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            var isTargetSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
            var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
            bool isSelfChange = currentUserName.Equals(changeRoleDto.UserName);

            if (isAdmin && (isTargetSuperAdmin || isTargetAdmin))
                throw new ForbiddenAccessException("Admins cannot change roles of SuperAdmins or other Admins!");
            if (isAdmin && changeRoleDto.Role != Role.Reader.ToString() && changeRoleDto.Role != Role.Writer.ToString())
                throw new ForbiddenAccessException("Admins can only assign Reader or Writer roles!");

            if (isSuperAdmin && (isSelfChange || isTargetSuperAdmin))
                throw new ForbiddenAccessException("SuperAdmins cannot change their own role or other SuperAdmins' roles!");
        }

        private async Task AuthorizeUpdateAsync(ApplicationUser currentUser, ApplicationUser targetUser,
            string targetUserName, string currentUserName)
        {
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            var isTargetSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
            var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");

            bool isSelfUpdate = currentUserName.Equals(targetUserName);

            if (isSelfUpdate)
                return;

            if (isAdmin && (isTargetSuperAdmin || isTargetAdmin))
                throw new ForbiddenAccessException("Admins cannot modify SuperAdmins or other Admins!");
            if (!isAdmin && !isSuperAdmin)
                throw new ForbiddenAccessException("You aren't authorized to perform this action!");
        }

        private async Task AuthorizeDeletionAsync(ApplicationUser currentUser, ApplicationUser targetUser,
            string userName, string currentUserName)
        {
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            var isTargetSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
            var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");

            bool isSelfDelete = currentUserName.Equals(userName);

            if (isSuperAdmin && isSelfDelete)
                throw new ForbiddenAccessException("SuperAdmins cannot delete their own account!");
            if (isSuperAdmin && isTargetSuperAdmin)
                throw new ForbiddenAccessException("SuperAdmins cannot delete other SuperAdmins!");
            if (isAdmin && (isTargetSuperAdmin || isTargetAdmin) && !isSelfDelete)
                throw new ForbiddenAccessException("Admins cannot delete SuperAdmins or other Admins!");
            if (!isAdmin && !isSuperAdmin && !isSelfDelete)
                throw new ForbiddenAccessException("You aren't authorized to perform this action!");
        }
    }
}
