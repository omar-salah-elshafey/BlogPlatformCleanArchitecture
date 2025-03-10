using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogPlatformCleanArchitecture.Application.Configurations;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using System.Text.RegularExpressions;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;
        private readonly ILogger<TokenService> _logger;
        public TokenService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, ILogger<TokenService> logger)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
        }
        public async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user)
        {
            var userClaim = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName)
            }.Union(userClaim).Union(roleClaims);

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.ToLocalTime().AddMinutes(_jwt.Lifetime),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        public async Task<AuthResponseModel> RefreshTokenAsync(string token)
        {
            var authResponseModel = new AuthResponseModel();
            token = Uri.UnescapeDataString(token); // Decode the token
            token = Regex.Replace(token, @"\s+", "+");
            _logger.LogError("Decoded Token: " + token);
            var user = await _userManager.Users.Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null)
                throw new InvalidTokenException("Invalid Token! user null");
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
            if (!refreshToken.IsActive)
                throw new InvalidTokenException("Invalid Token! inactive token");
            // Revoke current refresh token
            refreshToken.RevokedOn = DateTime.UtcNow.ToLocalTime();
            var newRefreshToken = await GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);
            await RemoveInactiveRefreshTokens(user);

            var jwtToken = await CreateJwtTokenAsync(user);
            authResponseModel.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authResponseModel.ExpiresAt = jwtToken.ValidTo;
            authResponseModel.Email = user.Email;
            authResponseModel.Username = user.UserName;
            authResponseModel.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
            authResponseModel.RefreshToken = newRefreshToken.Token;
            authResponseModel.RefreshTokenExpiresOn = newRefreshToken.ExpiresOn.ToLocalTime();
            return authResponseModel;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            _logger.LogError("refff: "+token);
            var user = await _userManager.Users.Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            _logger.LogError("user: " +  user.UserName);
            if (user == null)
                throw new InvalidTokenException("Token revocation failed: user not found.");
            
            var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
            
            if (!refreshToken.IsActive)
                throw new InvalidTokenException($"Token revocation failed: token already inactive for user {user.UserName}");
            
            refreshToken.RevokedOn = DateTime.UtcNow.ToLocalTime();
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError($"Token revocation failed for user {user.UserName} due to update failure.");
                throw new Exception("Failed to update user after token removal.");
            }
            _logger.LogInformation($"Token revoked successfully for user {user.UserName} at {DateTime.UtcNow.ToLocalTime()}");
        }


        public async Task<RefreshToken> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                createdOn = DateTime.UtcNow.ToLocalTime(),
                ExpiresOn = DateTime.Now.AddDays(7),
                Token = Convert.ToBase64String(randomNumber)
            };
        }

        public async Task RemoveInactiveRefreshTokens(ApplicationUser user)
        {
            user.RefreshTokens.RemoveAll(t => !t.IsActive);
            await _userManager.UpdateAsync(user);
        }

    }
}
