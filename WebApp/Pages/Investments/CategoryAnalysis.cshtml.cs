using Application.Features.Investments.GetCategoryPerformance;
using Application.Features.Portfolios.GetPortfolioById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Pages.Investments;

public class CategoryAnalysisModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryAnalysisModel> _logger;

    public List<CategoryPerformanceDto> Categories { get; private set; } = new();
    public int? PortfolioId { get; private set; }
    public string PortfolioName { get; private set; } = "All Portfolios";
    public bool IsAllPortfolios => !PortfolioId.HasValue || PortfolioId.Value == 0;
    
    // Chart data properties
    public string CategoryLabelsJson { get; private set; } = "[]";
    public string AllocationDataJson { get; private set; } = "[]";
    public string ReturnDataJson { get; private set; } = "[]";
    public string ChartColorsJson { get; private set; } = "[]";

    public CategoryAnalysisModel(IMediator mediator, ILogger<CategoryAnalysisModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int? portfolioId)
    {
        PortfolioId = portfolioId;

        // If specific portfolio is selected, get its name
        if (portfolioId.HasValue && portfolioId.Value > 0)
        {
            var portfolioResult = await _mediator.Send(new GetPortfolioByIdRequest { Id = portfolioId.Value });
            if (portfolioResult.IsSuccess)
            {
                PortfolioName = portfolioResult.Value.Name;
            }
        }

        // Get category performance data
        var categoryResult = await _mediator.Send(new GetCategoryPerformanceRequest { PortfolioId = portfolioId });
        if (categoryResult.IsSuccess)
        {
            Categories = categoryResult.Value;
            PrepareChartData();
        }
        else
        {
            _logger.LogError("Failed to retrieve category performance data");
        }

        return Page();
    }

    private void PrepareChartData()
    {
        if (Categories == null || !Categories.Any())
        {
            return;
        }

        // Generate random colors for chart segments
        var random = new Random();
        var colors = Categories.Select(_ => $"rgba({random.Next(0, 200)}, {random.Next(0, 200)}, {random.Next(0, 200)}, 0.8)").ToList();
        
        // Prepare data for charts
        var categoryLabels = Categories.Select(c => c.CategoryName).ToList();
        var allocationData = Categories.Select(c => c.AllocationPercentage).ToList();
        var returnData = Categories.Select(c => c.ReturnPercentage).ToList();
        
        // Serialize to JSON for JavaScript charts
        CategoryLabelsJson = System.Text.Json.JsonSerializer.Serialize(categoryLabels);
        AllocationDataJson = System.Text.Json.JsonSerializer.Serialize(allocationData);
        ReturnDataJson = System.Text.Json.JsonSerializer.Serialize(returnData);
        ChartColorsJson = System.Text.Json.JsonSerializer.Serialize(colors);
    }
} 