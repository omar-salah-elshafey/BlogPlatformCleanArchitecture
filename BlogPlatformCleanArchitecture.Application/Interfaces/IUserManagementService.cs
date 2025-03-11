using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<int> GetUsersCountAsync();
        Task<PaginatedResponseModel<UserDto>> GetUSersAsync(int pageNumber, int pageSize);
        Task<UserDto> GetUserProfileAsync(string userName);
        Task<PaginatedResponseModel<UserDto>> SearchUsersAsync(string searchQuery, int pageNumber, int pageSize);
        Task ChangeRoleAsync(ChangeUserRoleDto changeRoleDto);
        Task DeleteUserAsync(string UserName, string refreshToken, string password);
        Task<UpdateUserResponseModel> UpdateUserAsync(UpdateUserDto updateUserDto);
    }
}
