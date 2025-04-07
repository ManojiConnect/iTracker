using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;
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
    public IEnumerable<InvestmentResponse> Investments { get; set; } = new List<InvestmentResponse>();
    public List<CategoryDistributionItem> CategoryDistribution { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; }

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