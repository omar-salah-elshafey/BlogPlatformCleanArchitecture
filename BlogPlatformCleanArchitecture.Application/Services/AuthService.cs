﻿using BlogPlatformCleanArchitecture.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using BlogPlatformCleanArchitecture.Domain.Enums;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<DataProtectionTokenProviderOptions> _tokenProviderOptions;
        private readonly IEmailService _emailService;
        public readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly ICookieService _cookieService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOtpService _otpService;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<DataProtectionTokenProviderOptions> tokenProviderOptions, IEmailService emailService,
            ITokenService tokenService, ILogger<AuthService> logger, ICookieService cookieService,
            IHttpContextAccessor httpContextAccessor, IOtpService otpService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenProviderOptions = tokenProviderOptions;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger;
            _cookieService = cookieService;
            _httpContextAccessor = httpContextAccessor;
            _otpService = otpService;
        }

        public async Task<AuthResponseModel> RegisterUserAsync(RegistrationDto registrationDto)
        {
            _logger.LogWarning("Registering a new user...");
            var currentUnser = _httpContextAccessor.HttpContext?.User;
            if (currentUnser is not null)
            {
                var role = currentUnser.FindFirst(claim => claim.Type == ClaimTypes.Role)?.Value;
                if(role != null && role.ToLower() == "admin" && 
                    (registrationDto.Role.ToString().ToLower() == "admin" || registrationDto.Role.ToString().ToLower() == "superadmin"))
                {
                    throw new ForbiddenAccessException("An Admin Cannot add another admin or a SuperAdmin");
                }
            }

            if (await _userManager.FindByEmailAsync(registrationDto.Email) is not null)
                throw new DuplicateValueException("This Email is already used!");

            if (await _userManager.FindByNameAsync(registrationDto.UserName) is not null)
                throw new DuplicateValueException("This Username is already used!");

            var user = new ApplicationUser
            {
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                UserName = registrationDto.UserName,
                Email = registrationDto.Email,
                //EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, registrationDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                throw new UserCreationException($"Failed to create user: {errors}");
            }
            
            await _userManager.AddToRoleAsync(user, registrationDto.Role.ToString());
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var otp = await _otpService.GenerateAndStoreOtpAsync(user.Email, token);
            await _emailService.SendEmailAsync(
                user.Email,
                "Email Verification Code",
                $"Hello {user.UserName}, Use this OTP to verify your email: {otp}\nThis code is valid for 10 minutes."
            );

            _logger.LogInformation($"User with Email {user.Email} has been created Successfully!");
            return new AuthResponseModel
            {
                Email = user.Email,
                Username = user.UserName,
                Role = registrationDto.Role.ToString(),
            };
        }

        public async Task<AuthResponseModel> LoginAsync(LoginDto loginDto)
        {
            var authResponseModel = new AuthResponseModel();
            var user = await _userManager.FindByNameAsync(loginDto.EmailOrUserName)
               ?? await _userManager.FindByEmailAsync(loginDto.EmailOrUserName); //check if the user exists
            if (user == null || user.IsDeleted || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                _logger.LogWarning("Invalid Email or Password!");
                throw new InvalidCredentialsException("Invalid Email or Password!");                
            }
            if (!user.EmailConfirmed){
                _logger.LogWarning("Please Confirm Your Email First.");
                throw new EmailNotConfirmedException("Please Confirm Your Email First.");
            }

            var jwtSecurityToken = await _tokenService.CreateJwtTokenAsync(user);


            authResponseModel.Email = user.Email;
            authResponseModel.ExpiresAt = jwtSecurityToken.ValidTo;
            authResponseModel.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
            authResponseModel.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authResponseModel.Username = user.UserName;
            authResponseModel.IsConfirmed = true;
            user.IsActive = true;

            if (!user.RefreshTokens.Any(t => t.IsActive))
            {
                await _tokenService.RemoveInactiveRefreshTokens(user);
                var refreshToken = await _tokenService.GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                authResponseModel.RefreshToken = refreshToken.Token;
                authResponseModel.RefreshTokenExpiresOn = refreshToken.ExpiresOn;
            }
            else
            {
                var activeToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authResponseModel.RefreshToken = activeToken.Token;
                authResponseModel.RefreshTokenExpiresOn = activeToken.ExpiresOn;
            }
            _logger.LogInformation("User Logged in Successfully!");
            return authResponseModel;
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            refreshToken = Uri.UnescapeDataString(refreshToken); // Decode the token
            refreshToken = Regex.Replace(refreshToken, @"\s+", "+");
            _logger.LogError("Decoded Token: " + refreshToken);

            // Revoke the refresh token
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var userId = userClaims!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId!);
            _logger.LogInformation($"{userId} for user {user.UserName}");
            user!.IsActive = false;
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("User logged out successfully.");
            _cookieService.RemoveFromCookies("refreshToken");
            _cookieService.RemoveFromCookies("userName");
            _cookieService.RemoveFromCookies("userID");
            return true;
        }
    }
}
