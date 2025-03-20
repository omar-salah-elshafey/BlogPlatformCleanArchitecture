using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class PasswordManagementService : IPasswordManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordManagementService> _logger; 
        private readonly IOtpService _otpService;

        public PasswordManagementService(UserManager<ApplicationUser> userManager, IEmailService emailService,
            ILogger<PasswordManagementService> logger, IOtpService otpService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _otpService = otpService;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogError("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }

            var hashedPassword = _userManager.PasswordHasher.HashPassword(user, resetPasswordDto.NewPassword);
            user.PasswordHash = hashedPassword;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Password reset failed for user {Email}", user.Email);
                throw new InvalidTokenException("Password reset failed.");
            }

            _logger.LogInformation("Your password has been reset successfully.");
        }

        public async Task ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email.Trim());
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }

            if (changePasswordDto.CurrentPassword.Trim().Equals(changePasswordDto.NewPassword.Trim()))
            {
                _logger.LogWarning("New and old password cannot be the same!");
                throw new InvalidInputsException("New and old password cannot be the same!");
            }

            var result = await _userManager.ChangePasswordAsync(user, 
                changePasswordDto.CurrentPassword.Trim(), changePasswordDto.NewPassword.Trim());
            if (!result.Succeeded)
                throw new InvalidInputsException("The Current password is WRONG!");

            _logger.LogInformation("Your password has been updated successfully.") ;
        }

        public async Task ResetPasswordRequestAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var otp = await _otpService.GenerateAndStoreOtpAsync(email, token);


            await _emailService.SendEmailAsync(
                email,
                "Password Reset Code",
                $"Hello {user.UserName}, Use this OTP to reset your password: '{otp}'\nThis code is valid for 10 minutes."
            );
            _logger.LogInformation($"A Password Reset OTP has been sent to {email}");
        }

        public async Task VerifyResetPasswordTokenAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }

            var (realToken, isExpired) = await _otpService.GetTokenFromOtpAsync(confirmEmailDto.Email, confirmEmailDto.Token);
            if (realToken == null)
            {
                if (isExpired)
                {
                    _logger.LogWarning("OTP has expired for user {Email}", user.Email);
                    throw new InvalidTokenException("The OTP has expired. Please request a new one.");
                }
                _logger.LogWarning("Invalid OTP for user {Email}", user.Email);
                throw new InvalidTokenException("OTP is not valid.");
            }

            var result = await _userManager.VerifyUserTokenAsync(user, 
                _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", realToken);
            if (!result)
            {
                _logger.LogWarning("Token is not valid.");
                throw new InvalidTokenException("Token is not valid.");
            }

            _logger.LogInformation("Your Password reset request is verified.");
        }
    }
}
