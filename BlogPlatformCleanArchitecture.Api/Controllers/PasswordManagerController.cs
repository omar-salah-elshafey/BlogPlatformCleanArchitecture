using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordManagerController : ControllerBase
    {
        private readonly IPasswordManagementService _passwordManagementService;
        private readonly ILogger<PasswordManagerController> _logger;
        public PasswordManagerController(IPasswordManagementService passwordManagementService, 
            ILogger<PasswordManagerController> logger)
        {
            _passwordManagementService = passwordManagementService;
            _logger = logger;
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _passwordManagementService.ResetPasswordAsync(resetPasswordDto);
            return Ok(new { Message = "Your password has been reset successfully." });
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _passwordManagementService.ChangePasswordAsync(changePasswordDto);
            return Ok(new { Message = "Your password has been Changed successfully." });
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequestAsync(string email)
        {
            _logger.LogError("This is from the full"+email);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _passwordManagementService.ResetPasswordRequestAsync(email);
            return Ok(new { Message = "A Password Reset Code has been sent to your Email!" });
        }

        [HttpPost("verify-password-reset-token")]
        public async Task<IActionResult> VerifyResetPasswordTokenAsync(ConfirmEmailDto confirmEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Call the service to verify the email
            await _passwordManagementService.VerifyResetPasswordTokenAsync(confirmEmailDto);

            return Ok(new { Message = "Your Password reset request is verified." });
        }
    }
}
