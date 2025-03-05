using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class PasswordManagementService : IPasswordManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IOptions<DataProtectionTokenProviderOptions> _tokenProviderOptions;
        private readonly ILogger<PasswordManagementService> _logger;

        public PasswordManagementService(UserManager<ApplicationUser> userManager, IEmailService emailService,
            IOptions<DataProtectionTokenProviderOptions> tokenProviderOptions, ILogger<PasswordManagementService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenProviderOptions = tokenProviderOptions;
            _logger = logger;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogError("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }
                

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Token is not valid.");
                throw new InvalidTokenException("Token is not valid.");
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
            //generating the token to verify the user's email
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Dynamically get the expiration time from the options
            var expirationTime = _tokenProviderOptions.Value.TokenLifespan.TotalMinutes;

            await _emailService.SendEmailAsync(email, "Password Reset Code.",
                $"Hello {user.UserName}, Use this new token to Reset your Password: {token}\n This code is Valid only for {expirationTime} Minutes.");
            _logger.LogInformation("A Password Reset Code has been sent to your Email!");
        }

        public async Task VerifyResetPasswordTokenAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User not found, Email is incorrect!");
                throw new NotFoundException("User not found, Email is incorrect!");
            }
            var result = await _userManager.VerifyUserTokenAsync(user, 
                _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", confirmEmailDto.Token);
            if (!result)
            {
                _logger.LogWarning("Token is not valid.");
                throw new InvalidTokenException("Token is not valid.");
            }

            _logger.LogInformation("Your Password reset request is verified.");
        }
    }
}
