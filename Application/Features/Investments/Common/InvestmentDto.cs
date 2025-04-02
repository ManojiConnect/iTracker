using System;
using Domain.Entities;

namespace Application.Features.Investments.Common;

public class InvestmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string PortfolioName { get; set; } = string.Empty;
    public InvestmentCategory Category { get; set; } = null!;
    public Portfolio Portfolio { get; set; } = null!;
} 