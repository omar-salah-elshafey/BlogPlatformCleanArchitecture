using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserDto>> GetUSersAsync();
        Task<UserDto> GetUserProfileAsync();
        Task<List<UserDto>> SearchUsersAsync(string searchQuery);
        Task<string> ChangeRoleAsync(ChangeUserRoleDto changeRoleDto);
        Task<string> DeleteUserAsync(string UserName, string CurrentUserName, string refreshToken);
        Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto);
    }
}
