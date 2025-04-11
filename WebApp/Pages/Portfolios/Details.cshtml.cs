using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Portfolios;

public class CategoryDistributionItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
}

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;

    public PortfolioDto Portfolio { get; set; } = null!;
    public PaginatedList<InvestmentDto> Investments { get; set; } = null!;
    public List<CategoryDistributionItem> CategoryDistribution { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? SearchText { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    public DetailsModel(IMediator mediator, IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Load settings
        Settings = await _settingsService.GetSettingsAsync();
        
        var portfolioResult = await _mediator.Send(new GetPortfolioByIdRequest { Id = id });
        
        if (!portfolioResult.IsSuccess)
        {
            return NotFound();
        }

        Portfolio = portfolioResult.Value;

        var investmentsResult = await _mediator.Send(new GetPortfolioInvestmentsRequest { PortfolioId = id });
        if (investmentsResult.IsSuccess)
        {
            var allInvestments = investmentsResult.Value.ToList();
            
            // Apply filters if provided
            if (!string.IsNullOrEmpty(SearchText) || !string.IsNullOrEmpty(CategoryFilter))
            {
                var filteredItems = allInvestments.AsQueryable();
                
                // Apply search filter
                if (!string.IsNullOrEmpty(SearchText))
                {
                    filteredItems = filteredItems.Where(i => 
                        i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                        i.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }
                
                // Apply category filter
                if (!string.IsNullOrEmpty(CategoryFilter))
                {
                    filteredItems = filteredItems.Where(i => i.CategoryName == CategoryFilter);
                }
                
                allInvestments = filteredItems.ToList();
            }
            
            // Create paginated list
            var totalCount = allInvestments.Count();
            var pagedItems = allInvestments
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            Investments = new PaginatedList<InvestmentDto>(
                pagedItems,
                totalCount,
                PageNumber,
                PageSize);
            
            // Calculate category distribution
            if (allInvestments.Any())
            {
                var totalValue = allInvestments.Sum(i => i.CurrentValue);
                
                CategoryDistribution = allInvestments
                    .GroupBy(i => i.CategoryName ?? "Uncategorized")
                    .Select(g => new CategoryDistributionItem 
                    {
                        Name = g.Key,
                        Value = g.Sum(i => i.CurrentValue),
                        Percentage = totalValue > 0 ? (g.Sum(i => i.CurrentValue) / totalValue) * 100 : 0
                    })
                    .OrderByDescending(i => i.Value)
                    .ToList();
            }
        }
        
        // Check if this is an AJAX request
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            // Return only the investments list partial view
            return Partial("_InvestmentsList", Investments);
        }

        return Page();
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