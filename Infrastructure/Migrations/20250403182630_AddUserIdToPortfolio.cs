using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Portfolios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "SecurityStamp" },
                values: new object[] { "b06ad8ae-4d00-4f64-b19e-28801f8525bc", new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8950), "c4bce9d4-fada-41bb-b6a8-5fd90da48a4e" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8760));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8760));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8760));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8770));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 3, 18, 26, 29, 874, DateTimeKind.Utc).AddTicks(8770));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Portfolios");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "SecurityStamp" },
                values: new object[] { "4b3a7109-bbf6-4187-aa04-5b9c309467f8", new DateTime(2025, 4, 3, 16, 18, 49, 158, DateTimeKind.Utc).AddTicks(2260), "7f24b823-cc35-4154-89ea-6459fceb4c06" });

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
        }
    }
}
