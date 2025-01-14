﻿using BlogPlatformCleanArchitecture.Application.ExceptionHandling;

namespace BlogPlatformCleanArchitecture.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DuplicateEmailException ex)
            {
                _logger.LogWarning($"Duplicate email error: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (DuplicateUsernameException ex)
            {
                _logger.LogWarning($"Duplicate username error: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (UserCreationException ex)
            {
                _logger.LogError($"User creation error: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Application.ExceptionHandling.UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access error: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (EmailNotConfirmedException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (InvalidEmailOrTokenException ex)
            {
                _logger.LogWarning($"Invalid email or token: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (EmailAlreadyConfirmedException ex)
            {
                _logger.LogWarning($"Email already confirmed: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (InvalidTokenException ex)
            {
                _logger.LogWarning($"Invalid token: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
            }
        }
    }
}