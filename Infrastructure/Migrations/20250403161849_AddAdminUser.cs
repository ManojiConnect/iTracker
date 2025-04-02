using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Investments");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2c5e174e-3b0e-446f-86af-483d56fd7210", null, "Administrator", "ADMINISTRATOR" },
                    { "e1823908-c7c9-4e53-980e-972fd4799f59", null, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedBy", "CreatedDate", "Email", "EmailConfirmed", "FirstName", "IsActive", "Language", "LastModifiedBy", "LastModifiedDate", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfileUrl", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a", 0, "4b3a7109-bbf6-4187-aa04-5b9c309467f8", "System", new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2260), "Admin@itrackerApp.com", true, "Admin", true, "en", "", null, "User", false, null, "ADMIN@ITRACKERAPP.COM", "ADMIN@ITRACKERAPP.COM", "AQAAAAIAAYagAAAAEGTPKJTAQNLVgJUJU1Og0Z6qDZQqV2+GJ4dxP/e81kJHW+JgzcnGZRQdDadQNpUFxQ==", "1234567890", true, null, "7f24b823-cc35-4154-89ea-6459fceb4c06", false, "Admin@itrackerApp.com" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2110));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2110));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2110));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2120));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2120));

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "2c5e174e-3b0e-446f-86af-483d56fd7210", "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1823908-c7c9-4e53-980e-972fd4799f59");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2c5e174e-3b0e-446f-86af-483d56fd7210", "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "Investments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 2, 16, 7, 13, 830, DateTimeKind.Utc).AddTicks(9880));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 2, 16, 7, 13, 830, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 2, 16, 7, 13, 830, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 2, 16, 7, 13, 830, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 2, 16, 7, 13, 830, DateTimeKind.Utc).AddTicks(9890));
        }
    }
}
