using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Portfolio
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialValue { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime CreatedOn { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int? ModifiedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }

    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
} 