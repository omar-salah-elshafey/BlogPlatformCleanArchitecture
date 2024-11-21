﻿using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseModel> RegisterUserAsync(RegistrationDto registrationDto, string role);
        Task<AuthResponseModel> LoginAsync(LoginDto loginDto);
        Task<bool> LogoutAsync(string refreshToken, string userId);
    }
}