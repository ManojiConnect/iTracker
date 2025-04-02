using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Migrations;

public partial class AddInvestmentCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "InvestmentCategories",
            columns: new[] { "Id", "Name", "Description", "CreatedBy", "CreatedDate", "IsDelete", "LastModifiedBy", "LastModifiedDate" },
            values: new object[,]
            {
                { 1, "Mutual Funds", "Investment vehicles that pool money from multiple investors to invest in a diversified portfolio of securities", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 2, "Equity", "Direct investment in company stocks", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 3, "Commodity", "Investment in physical commodities like gold, silver, oil, etc.", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 4, "Fixed Income", "Investment in bonds and other debt instruments", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 5, "Real Estate", "Investment in property and real estate assets", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 6, "Cryptocurrency", "Digital or virtual currencies", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 7, "ETFs", "Exchange-Traded Funds that track various indices or sectors", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow },
                { 8, "Forex", "Foreign exchange market investments", "System", DateTime.UtcNow, false, "System", DateTime.UtcNow }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "InvestmentCategories",
            keyColumn: "Id",
            keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 });
    }
} 