using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Services;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatformCleanArchitecture.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with SQL Server configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnections")));

            // Register repositories if you have any, e.g., IUserRepository
            // services.AddScoped<IUserRepository, UserRepository>();

            // Register application services, e.g., AuthService
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordManagementService, PasswordManagementService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            return services;
        }
    }
}
