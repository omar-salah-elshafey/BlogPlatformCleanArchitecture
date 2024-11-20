﻿using System.IdentityModel.Tokens.Jwt;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface ITokenService
    {
        Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user);
        Task<AuthResponseModel> RefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
        Task<RefreshToken> GenerateRefreshToken();
    }
}
