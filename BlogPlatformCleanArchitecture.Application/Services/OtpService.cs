using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using System.Security.Cryptography;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly TimeSpan _otpLifespan = TimeSpan.FromMinutes(10);

        public OtpService(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        private string GenerateSecureSixDigitOtp()
        {
            byte[] randomBytes = new byte[4];
            RandomNumberGenerator.Fill(randomBytes);
            int randomValue = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
            int otp = 100000 + (randomValue % 900000);
            return otp.ToString();
        }

        public async Task<string> GenerateAndStoreOtpAsync(string email, string token)
        {
            await _otpRepository.DeleteOtpsByEmailAsync(email);

            var otpCode = GenerateSecureSixDigitOtp();
            var otp = new Otp
            {
                Email = email,
                OtpCode = otpCode,
                Token = token,
                ExpirationDateTime = DateTime.UtcNow + _otpLifespan
            };

            await _otpRepository.AddAsync(otp);
            return otpCode;
        }
        public async Task<(string Token, bool IsExpired)> GetTokenFromOtpAsync(string email, string otp)
        {
            var otpEntity = await _otpRepository.GetByEmailAndOtpAsync(email, otp);
            if (otpEntity == null)
            {
                return (null, false);
            }

            bool isExpired = otpEntity.ExpirationDateTime <= DateTime.UtcNow;
            string token = isExpired ? null : otpEntity.Token;

            await _otpRepository.DeleteAsync(otpEntity);

            return (token, isExpired);
        }
    }
}
