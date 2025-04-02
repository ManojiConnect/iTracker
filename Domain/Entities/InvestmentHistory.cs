using System;

namespace Domain.Entities;

public class InvestmentHistory
{
    public int Id { get; set; }
    public int InvestmentId { get; set; }
    public decimal Value { get; set; }
    public DateTime RecordedDate { get; set; }
    public string? Note { get; set; }
    
    // Navigation property
    public Investment Investment { get; set; } = null!;
} 