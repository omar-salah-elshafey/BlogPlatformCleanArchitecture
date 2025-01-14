using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task VerifyEmail(ConfirmEmailDto confirmEmailDto);
        Task ResendEmailConfirmationTokenAsync(string Email);
    }
}
