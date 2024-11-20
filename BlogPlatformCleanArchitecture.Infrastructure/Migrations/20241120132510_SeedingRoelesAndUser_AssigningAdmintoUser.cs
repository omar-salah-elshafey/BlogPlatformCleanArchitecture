using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlogPlatformCleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedingRoelesAndUser_AssigningAdmintoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a331b209-871f-45fc-9a8d-f357f9bff3b1", "1", "Admin", "ADMIN" },
                    { "b330b209-871f-45fc-9a8d-f357f9bff3b1", "2", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DateCreated", "Email", "EmailConfirmed", "FirstName", "IsActive", "IsDeleted", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "7e53a491-a9de-4c75-af44-ff3271a5176c", 0, "2a5e4ca9-34f7-48e4-aca2-66682043cd41", new DateTime(2024, 11, 20, 15, 25, 10, 324, DateTimeKind.Local).AddTicks(2864), "omarsalah@test.com", true, "Omar", false, false, "Salah", false, null, "OMARSALAH@TEST.COM", "OMAR_SALAH", "AQAAAAIAAYagAAAAEBvDZhumCKMM1b4T+EdGgAOYaA6TWioyC1vjw3VW0CUHsxh9hptMsRhMCeWSe6LeHw==", null, false, "c57364d2-f91b-4543-9f61-7a3a95b5c59a", false, "omar_salah" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "a331b209-871f-45fc-9a8d-f357f9bff3b1", "7e53a491-a9de-4c75-af44-ff3271a5176c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b330b209-871f-45fc-9a8d-f357f9bff3b1");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "a331b209-871f-45fc-9a8d-f357f9bff3b1", "7e53a491-a9de-4c75-af44-ff3271a5176c" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a331b209-871f-45fc-9a8d-f357f9bff3b1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7e53a491-a9de-4c75-af44-ff3271a5176c");
        }
    }
}
