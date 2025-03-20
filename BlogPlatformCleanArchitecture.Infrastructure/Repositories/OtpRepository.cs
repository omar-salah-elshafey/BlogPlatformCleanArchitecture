using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly ApplicationDbContext _context;

        public OtpRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Otp?> GetByEmailAndOtpAsync(string email, string otp)
        {
            return await _context.Otps
                .FirstOrDefaultAsync(o => o.Email == email && o.OtpCode == otp);
        }

        public async Task AddAsync(Otp otp)
        {
            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Otp otp)
        {
            _context.Otps.Remove(otp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOtpsByEmailAsync(string email)
        {
            var otps = _context.Otps.Where(o => o.Email == email);
            _context.Otps.RemoveRange(otps);
            await _context.SaveChangesAsync();
        }
    }
}
