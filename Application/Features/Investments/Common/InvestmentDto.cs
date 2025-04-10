using System;
using Domain.Entities;

namespace Application.Features.Investments.Common;

public class InvestmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public string? Notes { get; set; }
    public int PortfolioId { get; set; }
    public string PortfolioName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
} 