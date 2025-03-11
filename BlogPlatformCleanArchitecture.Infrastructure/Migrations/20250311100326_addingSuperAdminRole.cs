using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatformCleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addingSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d331b209-871f-45fc-9a8d-f357f9bff3b1", "4", "SuperAdmin", "SUPERADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d331b209-871f-45fc-9a8d-f357f9bff3b1");
        }
    }
}
