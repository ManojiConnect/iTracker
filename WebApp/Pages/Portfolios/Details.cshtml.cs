using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;
using Application.Features.Investments.Common;
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
    public IEnumerable<InvestmentDto> Investments { get; set; } = new List<InvestmentDto>();
    public List<CategoryDistributionItem> CategoryDistribution { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; }
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    // Pagination properties
    public int TotalPages { get; set; }
    public int StartItem { get; set; }
    public int EndItem { get; set; }
    public int TotalItems { get; set; }

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
            Investments = investmentsResult.Value;
            
            // Calculate category distribution
            if (Investments.Any())
            {
                var totalValue = Investments.Sum(i => i.CurrentValue);
                
                CategoryDistribution = Investments
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

            // Set pagination properties
            TotalItems = Investments.Count();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            StartItem = ((PageNumber - 1) * PageSize) + 1;
            EndItem = Math.Min(StartItem + PageSize - 1, TotalItems);
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