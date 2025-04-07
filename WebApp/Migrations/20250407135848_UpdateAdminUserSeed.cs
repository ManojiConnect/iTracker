using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "38196677-3317-47fa-91a2-b7f30e17c454");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1823908-c7c9-4e53-980e-972fd4799f59",
                column: "ConcurrencyStamp",
                value: "6b60b8f3-c7b7-41a8-81c0-14a477c0bf05");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "Email", "NormalizedEmail", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "eb8cacd2-40a9-41ac-80aa-4df41aecd98f", new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5830), "admin@itracker.com", "ADMIN@ITRACKER.COM", "ADMIN@ITRACKER.COM", "feb19777-b614-4fe4-ae0d-c3751d5db3cd", "admin@itracker.com" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 58, 48, 23, DateTimeKind.Utc).AddTicks(5720));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "eb96f4d5-7f69-4351-9588-babea5316f2b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1823908-c7c9-4e53-980e-972fd4799f59",
                column: "ConcurrencyStamp",
                value: "31542b5b-1126-45b2-bdba-d02ff5543f7e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "Email", "NormalizedEmail", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "440e1207-fa44-4d8f-b29d-afaef8534ca0", new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740), "Admin@itrackerApp.com", "ADMIN@ITRACKERAPP.COM", "ADMIN@ITRACKERAPP.COM", "fc7b4c10-f1f1-409b-919d-9c91c1a32fff", "Admin@itrackerApp.com" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 13, 55, 28, 181, DateTimeKind.Utc).AddTicks(8740));
        }
    }
}
