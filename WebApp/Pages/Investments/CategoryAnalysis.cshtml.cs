using Application.Features.Investments.GetCategoryPerformance;
using Application.Features.Portfolios.GetPortfolioById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Services;

namespace WebApp.Pages.Investments;

public class CategoryAnalysisModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryAnalysisModel> _logger;
    private readonly CurrencyFormatterService _currencyFormatter;
    private readonly IApplicationSettingsService _applicationSettings;

    public List<CategoryPerformanceDto> Categories { get; private set; } = new();
    public int? PortfolioId { get; private set; }
    public string PortfolioName { get; private set; } = "All Portfolios";
    public bool IsAllPortfolios => !PortfolioId.HasValue || PortfolioId.Value == 0;
    
    // Chart data properties
    public string CategoryLabelsJson { get; private set; } = "[]";
    public string AllocationDataJson { get; private set; } = "[]";
    public string ReturnDataJson { get; private set; } = "[]";
    public string ChartColorsJson { get; private set; } = "[]";

    // Currency formatting delegate
    public Func<decimal, string> FormatCurrency { get; private set; }

    public CategoryAnalysisModel(
        IMediator mediator, 
        ILogger<CategoryAnalysisModel> logger,
        CurrencyFormatterService currencyFormatter,
        IApplicationSettingsService applicationSettings)
    {
        _mediator = mediator;
        _logger = logger;
        _currencyFormatter = currencyFormatter;
        _applicationSettings = applicationSettings;
    }

    public async Task<IActionResult> OnGetAsync(int? portfolioId)
    {
        PortfolioId = portfolioId;
        
        try
        {
            // Setup currency formatter delegate for views - use the application settings service
            FormatCurrency = amount => _applicationSettings.FormatCurrency(amount);
            _logger.LogInformation("Currency formatter initialized for CategoryAnalysis page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing currency formatter");
            
            // Fallback to basic formatting
            var symbol = await _currencyFormatter.GetCurrencySymbolAsync();
            FormatCurrency = amount => $"{symbol}{amount:N2}";
        }

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
            // Filter out any categories with null or empty names (which might be deleted categories)
            Categories = categoryResult.Value
                .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName) && c.CategoryName != "Uncategorized")
                .ToList();
                
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

        // Generate random colors for chart segments with a seed based on category ID for consistency
        var colors = new List<string>();
        foreach (var category in Categories)
        {
            var random = new Random(category.CategoryId); // Use CategoryId as seed for consistent colors
            colors.Add($"rgba({random.Next(0, 200)}, {random.Next(0, 200)}, {random.Next(0, 200)}, 0.8)");
        }
        
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