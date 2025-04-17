using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "LastModifiedDate", "Role", "SecurityStamp" },
                values: new object[] { "e78bb8ab-0563-4a6f-b5cb-efa92b4395c0", new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480), new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480), "User", "7ea232ab-fedd-45f6-96a2-ccd9f66f5837" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "LastModifiedDate", "SecurityStamp" },
                values: new object[] { "3d3da8ec-8498-4c29-82a1-07be5f2dbf4d", new DateTime(2025, 4, 7, 15, 18, 5, 90, DateTimeKind.Utc).AddTicks(2670), new DateTime(2025, 4, 7, 15, 18, 5, 90, DateTimeKind.Utc).AddTicks(2670), "4024d517-ec10-468d-9682-db0de5819a0b" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 15, 18, 5, 90, DateTimeKind.Utc).AddTicks(2670));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 15, 18, 5, 90, DateTimeKind.Utc).AddTicks(2670));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 7, 15, 18, 5, 90, DateTimeKind.Utc).AddTicks(2670));
        }
    }
}
