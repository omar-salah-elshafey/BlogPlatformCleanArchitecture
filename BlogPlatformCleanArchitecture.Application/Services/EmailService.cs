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

        public EmailService(IConfiguration config, UserManager<ApplicationUser> userManager,
            IOptions<DataProtectionTokenProviderOptions> tokenProviderOptions, ILogger<EmailService> logger)
        {
            _config = config;
            _userManager = userManager;
            _tokenProviderOptions = tokenProviderOptions;
            _logger = logger;
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
                _logger.LogError("Email and token are required.");
                throw new InvalidEmailOrTokenException("Email and token are required."); 
            }

            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogError("User not found.");
                throw new UserNotFoundException("User not found.");
            }
                

            if (user.EmailConfirmed)
            {
                _logger.LogWarning("Your email is already confirmed.");
                throw new EmailAlreadyConfirmedException("Your email has been confirmed successfully.");
            }
                

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
            if (!result.Succeeded)
            {
                _logger.LogError("Token is not valid.");
                throw new InvalidTokenException("Token is not valid.");
            }

            _logger.LogInformation("Your email has been confirmed.");
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
                throw new UserNotFoundException("User not found.");
            }
                

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Your email is already confirmed.");
                throw new EmailAlreadyConfirmedException("Email is already confirmed.");
            }
                

            // Generate new token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var expirationTime = _tokenProviderOptions.Value.TokenLifespan.TotalMinutes;
            // Send the new token via email
            await SendEmailAsync(user.Email, "Email Verification Code",
                $"Hello {user.UserName}, Use this new token to verify your Email: {token}\n This code is Valid only for {expirationTime} Minutes.");

        }
    }
}
