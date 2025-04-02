using Domain.Entities.Common;
using System;

namespace Domain.Entities;

public class InvestmentTransaction : Entity
{
    public int InvestmentId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Units { get; set; }
    public decimal PricePerUnit { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
    public virtual Investment Investment { get; set; } = null!;
}

public enum TransactionType
{
    Buy,
    Sell,
    Dividend,
    Split
} 