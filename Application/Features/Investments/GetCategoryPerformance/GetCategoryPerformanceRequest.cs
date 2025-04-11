using MediatR;
using Ardalis.Result;
using System.Collections.Generic;

namespace Application.Features.Investments.GetCategoryPerformance;

public record GetCategoryPerformanceRequest : IRequest<Result<List<CategoryPerformanceDto>>>
{
    public int? PortfolioId { get; init; }
    public bool IncludeAllPortfolios => !PortfolioId.HasValue || PortfolioId.Value == 0;
} 