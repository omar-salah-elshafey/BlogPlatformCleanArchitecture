using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public readonly IAuthService _authService;
        public EmailController(IEmailService emailService, IAuthService authService)
        {
            _emailService = emailService;
            _authService = authService;
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> VerifyEmail(ConfirmEmailDto confirmEmailDto)
        {
            await _emailService.VerifyEmail(confirmEmailDto);

            return Ok(new { Message = "Your email has been confirmed successfully." });
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            await _emailService.ResendEmailConfirmationTokenAsync(Email);
            return Ok(new { Message = "A new verification email has been sent." });
        }
    }
}
