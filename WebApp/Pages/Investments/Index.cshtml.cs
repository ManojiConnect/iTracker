using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Investments.GetAllInvestments;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using Application.Features.Portfolios.GetAllPortfolios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Investments;

public class CategorySummary
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int InvestmentCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
}

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexModel> _logger;
    private readonly IApplicationSettingsService _settingsService;
    
    public PaginatedList<InvestmentDto> Investments { get; set; } = null!;
    public List<CategorySummary> CategorySummaries { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<PortfolioDto> Portfolios { get; set; } = new();
    public decimal TotalPortfolioValue { get; set; }
    public SystemSettingsViewModel Settings { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public IndexModel(
        IMediator mediator,
        ILogger<IndexModel> logger,
        IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _logger = logger;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Load settings
        Settings = await _settingsService.GetSettingsAsync();
        
        _logger.LogInformation("Fetching all investments");
        
        var result = await _mediator.Send(new GetAllInvestmentsRequest 
        { 
            PageNumber = PageNumber,
            PageSize = PageSize
        });
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch investments: {Error}", result.Error);
            return NotFound();
        }

        Investments = result.Value;
        CalculateCategorySummaries();
        
        // Load portfolios for the filter dropdown
        var portfoliosResult = await _mediator.Send(new GetAllPortfoliosRequest());
        if (portfoliosResult.IsSuccess)
        {
            Portfolios = portfoliosResult.Value.ToList();
        }
        
        return Page();
    }
    
    private void CalculateCategorySummaries()
    {
        // Group investments by category
        var categoryGroups = Investments.Items.GroupBy(i => i.CategoryName);
        
        // Calculate summaries for each category
        foreach (var group in categoryGroups)
        {
            var summary = new CategorySummary
            {
                Name = group.Key,
                Color = GetCategoryColor(group.Key),
                InvestmentCount = group.Count(),
                TotalValue = group.Sum(i => i.CurrentValue),
                UnrealizedGainLoss = group.Sum(i => i.UnrealizedGainLoss),
                ReturnPercentage = group.Sum(i => i.TotalInvestment) > 0
                    ? (group.Sum(i => i.UnrealizedGainLoss) / group.Sum(i => i.TotalInvestment)) * 100
                    : 0
            };
            
            CategorySummaries.Add(summary);
        }
        
        // Sort categories by total value
        CategorySummaries = CategorySummaries.OrderByDescending(c => c.TotalValue).ToList();
        
        // Get unique category names for filter dropdown
        Categories = CategorySummaries.Select(c => c.Name).ToList();
        
        // Calculate total portfolio value
        TotalPortfolioValue = CategorySummaries.Sum(c => c.TotalValue);
    }
    
    private string GetCategoryColor(string categoryName)
    {
        // Define colors for different categories
        return categoryName.ToLower() switch
        {
            "stocks" => "#4CAF50",
            "bonds" => "#2196F3",
            "real estate" => "#FF9800",
            "cryptocurrency" => "#9C27B0",
            "commodities" => "#F44336",
            "cash" => "#607D8B",
            _ => "#757575"
        };
    }
    
    public string FormatCurrency(decimal amount)
    {
        return _settingsService.FormatCurrency(amount);
    }
    
    public string FormatNumber(decimal number, int? decimalPlaces = null)
    {
        return _settingsService.FormatNumber(number, decimalPlaces);
    }
} 