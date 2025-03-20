using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface IOtpRepository
    {
        Task<Otp?> GetByEmailAndOtpAsync(string email, string otp);
        Task AddAsync(Otp otp);
        Task DeleteAsync(Otp otp);
        Task DeleteOtpsByEmailAsync(string email);
    }
}
