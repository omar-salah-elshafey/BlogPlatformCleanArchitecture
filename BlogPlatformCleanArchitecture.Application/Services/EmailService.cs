using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<DataProtectionTokenProviderOptions> _tokenProviderOptions;
        private readonly ILogger<EmailService> _logger;
        private readonly IOtpService _otpService;

        public EmailService(IConfiguration config, UserManager<ApplicationUser> userManager,
            IOptions<DataProtectionTokenProviderOptions> tokenProviderOptions, ILogger<EmailService> logger, IOtpService otpService)
        {
            _config = config;
            _userManager = userManager;
            _tokenProviderOptions = tokenProviderOptions;
            _logger = logger;
            _otpService = otpService;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task VerifyEmail(ConfirmEmailDto confirmEmailDto)
        {

            if (string.IsNullOrEmpty(confirmEmailDto.Email) || string.IsNullOrEmpty(confirmEmailDto.Token))
            {
                _logger.LogError("Email and OTP are required.");
                throw new InvalidEmailOrTokenException("Email and OTP are required."); 
            }

            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogError("User not found.");
                throw new NotFoundException("User not found.");
            }
                

            if (user.EmailConfirmed)
            {
                _logger.LogWarning("Your email is already confirmed.");
                throw new EmailAlreadyConfirmedException("Your email has been confirmed successfully.");
            }

            var (realToken, isExpired) = await _otpService.GetTokenFromOtpAsync(confirmEmailDto.Email, confirmEmailDto.Token);
            if (realToken == null)
            {
                if (isExpired)
                {
                    _logger.LogWarning("OTP has expired for user {Email}", user.Email);
                    throw new InvalidTokenException("The OTP has expired. Please request a new one.");
                }
                _logger.LogError("Invalid OTP for user {Email}", user.Email);
                throw new InvalidTokenException("The OTP is not valid.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, realToken);
            if (!result.Succeeded)
            {
                _logger.LogError("Token verification failed for user {Email}", user.Email);
                throw new InvalidTokenException("Token verification failed.");
            }

            _logger.LogInformation("Email confirmed successfully for user {Email}", user.Email);
        }

        public async Task ResendEmailConfirmationTokenAsync(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                _logger.LogError("Email is required.");
                throw new InvalidEmailOrTokenException("Email is required.");
            }
                

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogError("User not found.");
                throw new NotFoundException("User not found.");
            }
                

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Your email is already confirmed.");
                throw new EmailAlreadyConfirmedException("Email is already confirmed.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var otp = await _otpService.GenerateAndStoreOtpAsync(user.Email, token);
            await SendEmailAsync(
                user.Email,
                "Email Verification Code",
                $"Hello {user.UserName}, Use this OTP to verify your email: '{otp}'\nThis code is valid for 10 minutes."
            );

            _logger.LogInformation("New OTP sent to {Email}", user.Email);
        }
    }
}
