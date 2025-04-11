namespace Application.Features.Investments.GetCategoryPerformance;

public record CategoryPerformanceDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalInvestment { get; init; }
    public decimal CurrentValue { get; init; }
    public decimal ProfitLoss => CurrentValue - TotalInvestment;
    public decimal ReturnPercentage => TotalInvestment > 0 ? (CurrentValue - TotalInvestment) / TotalInvestment * 100 : 0;
    public decimal AllocationPercentage { get; init; }
    public int InvestmentCount { get; init; }
    public int PortfolioCount { get; init; } // Only meaningful when viewing all portfolios
} 