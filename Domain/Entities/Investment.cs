using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Entities;

/// <summary>
/// Represents an individual investment within a portfolio. An investment tracks the performance,
/// transactions, and historical values of a specific asset or security.
/// </summary>
public class Investment : Entity
{
    /// <summary>
    /// Gets or sets the name of the investment.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the trading symbol or identifier for the investment.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the portfolio this investment belongs to.
    /// </summary>
    public int PortfolioId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the investment category.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the total amount invested in this investment.
    /// </summary>
    public decimal TotalInvestment { get; set; }

    /// <summary>
    /// Gets or sets the current market value of the investment.
    /// </summary>
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the unrealized gain or loss of the investment.
    /// </summary>
    public decimal UnrealizedGainLoss { get; set; }

    /// <summary>
    /// Gets or sets the return percentage of the investment.
    /// </summary>
    public decimal ReturnPercentage { get; set; }

    /// <summary>
    /// Gets or sets the date when the investment was purchased.
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// Gets or sets the price per unit at the time of purchase.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Gets or sets any additional notes or comments about the investment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the portfolio this investment belongs to.
    /// </summary>
    public virtual Portfolio Portfolio { get; set; } = null!;

    /// <summary>
    /// Gets or sets the category of this investment.
    /// </summary>
    public virtual InvestmentCategory Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of transactions related to this investment.
    /// </summary>
    public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();

    /// <summary>
    /// Gets or sets the collection of historical values for this investment.
    /// </summary>
    public virtual ICollection<InvestmentHistory> HistoricalValues { get; set; } = new List<InvestmentHistory>();
} 