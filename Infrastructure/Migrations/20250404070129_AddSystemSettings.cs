using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrencySymbol = table.Column<string>(type: "TEXT", nullable: false),
                    DecimalSeparator = table.Column<string>(type: "TEXT", nullable: false),
                    ThousandsSeparator = table.Column<string>(type: "TEXT", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "INTEGER", nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", nullable: false),
                    FinancialYearStartMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultPortfolioView = table.Column<string>(type: "TEXT", nullable: false),
                    PerformanceCalculationMethod = table.Column<string>(type: "TEXT", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MinPasswordLength = table.Column<int>(type: "INTEGER", nullable: false),
                    SettingKey = table.Column<string>(type: "TEXT", nullable: false),
                    SettingValue = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", precision: 0, nullable: false),
                    ModifiedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", precision: 0, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDelete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9f8e9a5c-1d2e-4b3f-8a7c-6d5e4f3c2b1a",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "SecurityStamp" },
                values: new object[] { "e83b3ebc-c2f9-4f5a-9d51-39cf1038bcb9", new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3430), "fc75dca9-471c-4642-8914-29551307b5f5" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3240));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 4, 7, 1, 29, 511, DateTimeKind.Utc).AddTicks(3240));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

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
    }
}
