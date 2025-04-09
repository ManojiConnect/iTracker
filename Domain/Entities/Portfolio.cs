using System;
using System.Collections.Generic;

namespace Domain.Entities;

/// <summary>
/// Represents a portfolio of investments owned by a user. A portfolio tracks the total value,
/// investments, and performance metrics of a collection of investments.
/// </summary>
public class Portfolio
{
    /// <summary>
    /// Gets or sets the unique identifier for the portfolio.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the portfolio.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the portfolio.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial value of the portfolio when it was created.
    /// </summary>
    public decimal InitialValue { get; set; }

    /// <summary>
    /// Gets or sets the current total value of the portfolio, including all investments.
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Gets or sets the total amount invested in the portfolio.
    /// </summary>
    public decimal TotalInvestment { get; set; }

    /// <summary>
    /// Gets or sets the unrealized gain or loss of the portfolio.
    /// </summary>
    public decimal UnrealizedGainLoss { get; set; }

    /// <summary>
    /// Gets or sets the return percentage of the portfolio.
    /// </summary>
    public decimal ReturnPercentage { get; set; }

    /// <summary>
    /// Gets or sets the date when the portfolio was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the portfolio.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date when the portfolio was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the portfolio.
    /// </summary>
    public int? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the portfolio is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the portfolio has been soft-deleted.
    /// </summary>
    public bool IsDelete { get; set; }

    /// <summary>
    /// Gets or sets the Identity user ID of the portfolio owner.
    /// </summary>
    public string UserId { get; set; } = string.Empty; // The Identity user ID of the owner

    /// <summary>
    /// Gets or sets the collection of investments in this portfolio.
    /// </summary>
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
} 