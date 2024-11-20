using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserDto>> GetUSersAsync();
        Task<string> ChangeRoleAsync(ChangeUserRoleDto changeRoleDto);
        Task<string> DeleteUserAsync(string UserName, string CurrentUserName, string refreshToken);
        Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto);
    }
}
