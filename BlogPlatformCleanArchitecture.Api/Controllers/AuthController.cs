﻿using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ICookieService _cookieService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager,
            ITokenService tokenService, ICookieService cookieService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _tokenService = tokenService;
            _cookieService = cookieService;
            _logger = logger;
        }

        [HttpPost("register-reader")]
        public async Task<IActionResult> RegisterReaderAsync([FromBody] RegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterUserAsync(registrationDto, "Reader");

            return Ok(new
            {
                result.Email,
                result.Username,
                result.Role,
                result.IsAuthenticated,
                result.IsConfirmed,
                result.Message,
            });
        }

        [HttpPost("register-author")]
        public async Task<IActionResult> RegisterAuthorAsync([FromBody] RegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterUserAsync(registrationDto, "Author");

            return Ok(new
            {
                result.Email,
                result.Username,
                result.Role,
                result.IsAuthenticated,
                result.IsConfirmed,
                result.Message,
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterUserAsync(registrationDto, "Admin");

            return Ok(new
            {
                result.Email,
                result.Username,
                result.Role,
                result.IsAuthenticated,
                result.IsConfirmed,
                result.Message,
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.LoginAsync(loginDto);
            var user = await _userManager.FindByNameAsync(loginDto.EmailOrUserName)
               ?? await _userManager.FindByEmailAsync(loginDto.EmailOrUserName);

            //if (!string.IsNullOrEmpty(result.RefreshToken))
            //{
            //    _cookieService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
            //}
            _cookieService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
            _cookieService.SetUserIdCookie(user!.Id);
            _cookieService.SetUserNameCookie(user.UserName!);
            return Ok(new
            {
                result.AccessToken,
                result.ExpiresAt,
                result.RefreshToken,
                result.RefreshTokenExpiresOn,
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(string? refreshToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (refreshToken is null)
            {
                refreshToken = Request.Cookies["refreshToken"];
            }
            _logger.LogError("This is the token from the frontEnd: " + refreshToken);
            var userId = Request.Cookies["userId"];
            var userName = Request.Cookies["UserName"];
            var result = await _authService.LogoutAsync(refreshToken);
            if (!result)
                return BadRequest(result);
            return Ok(new {message= "Successfully logged out" });
        }

        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken(string? refreshToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (refreshToken is null)
            {
                refreshToken = Request.Cookies["refreshToken"];
            }
            _logger.LogError("This is the token from the frontEnd: " + refreshToken);
            var result = await _tokenService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            _cookieService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
            return Ok(new
            {
                result.AccessToken,
                result.ExpiresAt,
                result.RefreshToken,
                result.RefreshTokenExpiresOn,
            });
        }
    }
}
