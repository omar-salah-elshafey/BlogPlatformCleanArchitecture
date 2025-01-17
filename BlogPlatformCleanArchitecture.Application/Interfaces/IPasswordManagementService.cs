using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IPasswordManagementService
    {
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task ResetPasswordRequestAsync(string email);
        Task VerifyResetPasswordTokenAsync(ConfirmEmailDto confirmEmailDto);
    }
}
