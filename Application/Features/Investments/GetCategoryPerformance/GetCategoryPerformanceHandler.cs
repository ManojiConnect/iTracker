using Ardalis.Result;
using Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Investments.GetCategoryPerformance;

public class GetCategoryPerformanceHandler : IRequestHandler<GetCategoryPerformanceRequest, Result<List<CategoryPerformanceDto>>>
{
    private readonly IContext _dbContext;
    private readonly ILogger<GetCategoryPerformanceHandler> _logger;

    public GetCategoryPerformanceHandler(IContext dbContext, ILogger<GetCategoryPerformanceHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<List<CategoryPerformanceDto>>> Handle(GetCategoryPerformanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting category performance data for {PortfolioId}", 
                request.IncludeAllPortfolios ? "All Portfolios" : request.PortfolioId);

            // Get investments based on portfolio selection
            var query = _dbContext.Investments
                .Include(i => i.Category)
                .Include(i => i.Portfolio)
                .Where(i => !i.IsDelete && (i.Category == null || i.Category.IsDelete != true))
                .AsQueryable();

            if (!request.IncludeAllPortfolios)
            {
                query = query.Where(i => i.PortfolioId == request.PortfolioId);
            }

            var investments = await query.ToListAsync(cancellationToken);
            
            if (!investments.Any())
            {
                return Result<List<CategoryPerformanceDto>>.Success(new List<CategoryPerformanceDto>());
            }

            // Calculate total investment and current value for allocation percentages
            decimal totalInvestment = investments.Sum(i => i.TotalInvestment);
            decimal totalCurrentValue = investments.Sum(i => i.CurrentValue);

            // Group investments by category - properly handle null or deleted categories
            var categoryGroups = investments
                .GroupBy(i => new { 
                    CategoryId = i.Category?.Id ?? 0, 
                    CategoryName = (i.Category != null && !i.Category.IsDelete) ? i.Category.Name : "Uncategorized" 
                })
                .Select(g => new
                {
                    g.Key.CategoryId,
                    g.Key.CategoryName,
                    Investments = g.ToList(),
                    TotalInvestment = g.Sum(i => i.TotalInvestment),
                    CurrentValue = g.Sum(i => i.CurrentValue),
                    InvestmentCount = g.Count(),
                    PortfolioCount = g.Select(i => i.PortfolioId).Distinct().Count()
                })
                .ToList();

            // Convert to DTOs with calculated metrics
            var categoryPerformance = categoryGroups
                .Select(g => new CategoryPerformanceDto
                {
                    CategoryId = g.CategoryId,
                    CategoryName = g.CategoryName,
                    TotalInvestment = g.TotalInvestment,
                    CurrentValue = g.CurrentValue,
                    AllocationPercentage = totalInvestment > 0 
                        ? g.TotalInvestment / totalInvestment * 100 
                        : 0,
                    InvestmentCount = g.InvestmentCount,
                    PortfolioCount = g.PortfolioCount
                })
                .OrderByDescending(c => c.AllocationPercentage)
                .ToList();

            return Result<List<CategoryPerformanceDto>>.Success(categoryPerformance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category performance data");
            return Result<List<CategoryPerformanceDto>>.Error("Error retrieving category performance data");
        }
    }
} 