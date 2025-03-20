namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(string email, string token);
        Task<(string Token, bool IsExpired)> GetTokenFromOtpAsync(string email, string otp);
    }
}
