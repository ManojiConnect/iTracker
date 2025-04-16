using Domain.Entities.Common;
using System.Collections.Generic;

namespace Domain.Entities;

public class InvestmentCategory : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; } = string.Empty;
    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();
} 