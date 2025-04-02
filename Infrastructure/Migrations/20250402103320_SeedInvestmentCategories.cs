using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInvestmentCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InvestmentCategories",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "Description", "IsActive", "IsDelete", "ModifiedBy", "ModifiedOn", "Name" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 4, 2, 10, 33, 20, 293, DateTimeKind.Utc).AddTicks(8650), "Equity investments in publicly traded companies", true, false, null, null, "Stocks" },
                    { 2, 1, new DateTime(2025, 4, 2, 10, 33, 20, 293, DateTimeKind.Utc).AddTicks(8650), "Fixed income securities", true, false, null, null, "Bonds" },
                    { 3, 1, new DateTime(2025, 4, 2, 10, 33, 20, 293, DateTimeKind.Utc).AddTicks(8650), "Property and REITs", true, false, null, null, "Real Estate" },
                    { 4, 1, new DateTime(2025, 4, 2, 10, 33, 20, 293, DateTimeKind.Utc).AddTicks(8650), "Digital assets and tokens", true, false, null, null, "Cryptocurrency" },
                    { 5, 1, new DateTime(2025, 4, 2, 10, 33, 20, 293, DateTimeKind.Utc).AddTicks(8650), "Managed investment pools", true, false, null, null, "Mutual Funds" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
