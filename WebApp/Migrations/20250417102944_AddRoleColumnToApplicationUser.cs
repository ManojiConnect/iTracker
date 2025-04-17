using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleColumnToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "LastModifiedDate", "SecurityStamp" },
                values: new object[] { "2f68470c-b28f-499d-a51d-760b541ecc50", new DateTime(2025, 4, 17, 10, 29, 44, 406, DateTimeKind.Utc).AddTicks(9550), new DateTime(2025, 4, 17, 10, 29, 44, 406, DateTimeKind.Utc).AddTicks(9550), "77f19d9a-dfd6-43ca-8a05-4462c34790ab" });

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 29, 44, 406, DateTimeKind.Utc).AddTicks(9550));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 29, 44, 406, DateTimeKind.Utc).AddTicks(9550));

            migrationBuilder.UpdateData(
                table: "InvestmentCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2025, 4, 17, 10, 29, 44, 406, DateTimeKind.Utc).AddTicks(9550));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "LastModifiedDate", "SecurityStamp" },
                values: new object[] { "e78bb8ab-0563-4a6f-b5cb-efa92b4395c0", new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480), new DateTime(2025, 4, 17, 10, 15, 8, 314, DateTimeKind.Utc).AddTicks(2480), "7ea232ab-fedd-45f6-96a2-ccd9f66f5837" });

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
    }
}
