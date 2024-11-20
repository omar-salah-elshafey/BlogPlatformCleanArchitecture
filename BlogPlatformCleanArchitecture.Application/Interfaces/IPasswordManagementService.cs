using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IPasswordManagementService
    {
        Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<string> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<string> ResetPasswordRequestAsync(string email);
        Task<ResetPasswordResponseModel> VerifyResetPasswordRequestAsync(ConfirmEmailDto confirmEmailDto);
    }
}
