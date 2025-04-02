using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Investment : Entity
{
    public required string Name { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int PortfolioId { get; set; }
    public int CategoryId { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public string? Notes { get; set; }
    public virtual Portfolio Portfolio { get; set; } = null!;
    public virtual InvestmentCategory Category { get; set; } = null!;
    public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();
    public virtual ICollection<InvestmentHistory> HistoricalValues { get; set; } = new List<InvestmentHistory>();
} 