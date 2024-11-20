using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatformCleanArchitecture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _authService = authService;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterReaderAsync([FromBody] RegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterUserAsync(registrationDto, "User");

            if (!result.IsAuthenticated)
                return Ok(result.Message);

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

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

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
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
            }
            SetUserIdCookie(user.Id);
            SetUserNameCookie(user.UserName);
            return Ok(new
            {
                result.AccessToken,
                result.ExpiresAt
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userId = Request.Cookies["userId"];
            var userName = Request.Cookies["UserName"];
            var result = await _authService.LogoutAsync(refreshToken, userId);
            if (!result)
                return BadRequest(result);

            RemoveRefreshTokenCookie(refreshToken);
            RemoveUserIdCookie(userId);
            RemoveUserNameCookie(userName);
            return Ok("Successfully logged out");
        }

        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _tokenService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
            return Ok(new
            {
                result.AccessToken,
                result.ExpiresAt,
                result.RefreshToken,
                result.RefreshTokenExpiresOn,
            });
        }


        private void SetRefreshTokenCookie(string refreshToken, DateTime ex)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = ex.ToLocalTime(),
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void SetUserIdCookie(string userId)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("userID", userId, cookieOptions);
        }

        private void SetUserNameCookie(string userName)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("userName", userName, cookieOptions);
        }

        private void RemoveRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1).ToLocalTime(),
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("refreshToken", "", cookieOptions);
        }

        private void RemoveUserIdCookie(string userId)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1).ToLocalTime(),
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("userID", "", cookieOptions);
        }

        private void RemoveUserNameCookie(string userName)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1).ToLocalTime(),
                Secure = true,    // Set this in production when using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("userName", "", cookieOptions);
        }
    }
}
