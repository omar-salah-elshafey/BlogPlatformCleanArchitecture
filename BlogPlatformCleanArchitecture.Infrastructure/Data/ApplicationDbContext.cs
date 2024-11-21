﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            seedRoles(builder);
            addUser(builder);
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "7e53a491-a9de-4c75-af44-ff3271a5176c", // Admin user ID
                    RoleId = "a331b209-871f-45fc-9a8d-f357f9bff3b1"  // Admin role ID
                }
            );
        }

        private static void seedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "a331b209-871f-45fc-9a8d-f357f9bff3b1", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin".ToUpper() },
                new IdentityRole() { Id = "b330b209-871f-45fc-9a8d-f357f9bff3b1", Name = "User", ConcurrencyStamp = "2", NormalizedName = "User".ToUpper() }
                );
        }

        private static void addUser(ModelBuilder modelBuilder)
        {
            var adminUser = new ApplicationUser()
            {
                Id = "7e53a491-a9de-4c75-af44-ff3271a5176c",
                FirstName = "Omar",
                LastName = "Salah",
                UserName = "omar_salah",
                Email = "omarsalah@test.com",
                EmailConfirmed = true,
                NormalizedUserName = "OMAR_SALAH",
                NormalizedEmail = "omarsalah@test.com".ToUpper(),

            };
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "P@ssw0rd");
            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);
        }
    }
}