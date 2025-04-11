using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Portfolios.GetAllPortfolios;
using MediatR;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Portfolios;

public class PortfolioSummary
{
    public int PortfolioCount { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal TotalValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public int InvestmentCount { get; set; }
}

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;

    public List<PortfolioDto> Portfolios { get; set; } = new();
    public PortfolioSummary Summary { get; set; } = new();

    public IndexModel(
        ILogger<IndexModel> logger,
        IMediator mediator,
        IApplicationSettingsService settingsService)
    {
        _logger = logger;
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var result = await _mediator.Send(new GetAllPortfoliosRequest());
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve portfolios");
                return StatusCode(500, "Failed to retrieve portfolios");
            }

            Portfolios = result.Value.ToList();
            
            // Calculate summary
            Summary = new PortfolioSummary
            {
                PortfolioCount = Portfolios.Count,
                TotalInvestment = Portfolios.Sum(p => p.TotalInvested),
                TotalValue = Portfolios.Sum(p => p.TotalValue),
                InvestmentCount = Portfolios.Sum(p => p.InvestmentCount)
            };
            
            Summary.UnrealizedGainLoss = Summary.TotalValue - Summary.TotalInvestment;
            Summary.ReturnPercentage = Summary.TotalInvestment > 0 
                ? (Summary.UnrealizedGainLoss / Summary.TotalInvestment) * 100 
                : 0;

            return Page();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving portfolios");
            return StatusCode(500, "An error occurred while retrieving portfolios");
        }
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