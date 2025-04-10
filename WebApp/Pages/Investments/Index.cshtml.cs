using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Investments.GetAllInvestments;
using Application.Features.Investments.Common;
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
    public decimal TotalValue { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public int InvestmentCount { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexModel> _logger;
    private readonly IApplicationSettingsService _settingsService;
    
    public IEnumerable<InvestmentDto> Investments { get; set; } = new List<InvestmentDto>();
    public List<CategorySummary> CategorySummaries { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public decimal TotalPortfolioValue { get; set; }
    public SystemSettingsViewModel Settings { get; set; }

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
        
        var result = await _mediator.Send(new GetAllInvestmentsRequest());
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch investments: {Error}", result.Error);
            return NotFound();
        }

        Investments = result.Value;
        CalculateCategorySummaries();
        
        return Page();
    }
    
    private void CalculateCategorySummaries()
    {
        var groupedInvestments = Investments
            .GroupBy(i => i.CategoryName ?? "Uncategorized")
            .Select(g => new CategorySummary
            {
                Name = g.Key,
                TotalValue = g.Sum(i => i.CurrentValue),
                TotalInvestment = g.Sum(i => i.TotalInvestment),
                UnrealizedGainLoss = g.Sum(i => i.UnrealizedGainLoss),
                ReturnPercentage = g.Sum(i => i.TotalInvestment) > 0 
                    ? (g.Sum(i => i.UnrealizedGainLoss) / g.Sum(i => i.TotalInvestment)) * 100 
                    : 0,
                InvestmentCount = g.Count(),
                Color = GetCategoryColor(g.Key)
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();

        CategorySummaries = groupedInvestments;
        Categories = groupedInvestments.Select(c => c.Name).ToList();
        TotalPortfolioValue = groupedInvestments.Sum(c => c.TotalValue);
    }
    
    private string GetCategoryColor(string category)
    {
        return category.ToLower() switch
        {
            "stock" => "#4CAF50",
            "real estate" => "#2196F3",
            "crypto" => "#FF9800",
            "bond" => "#9C27B0",
            "mutual fund" => "#E91E63",
            "etf" => "#00BCD4",
            "other" => "#607D8B",
            _ => "#9E9E9E"
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