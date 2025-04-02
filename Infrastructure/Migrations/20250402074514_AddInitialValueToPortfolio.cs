using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialValueToPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalGainLoss",
                table: "Portfolios",
                newName: "UnrealizedGainLoss");

            migrationBuilder.AddColumn<decimal>(
                name: "InitialValue",
                table: "Portfolios",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInvestment",
                table: "Portfolios",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialValue",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "TotalInvestment",
                table: "Portfolios");

            migrationBuilder.RenameColumn(
                name: "UnrealizedGainLoss",
                table: "Portfolios",
                newName: "TotalGainLoss");
        }
    }
}
