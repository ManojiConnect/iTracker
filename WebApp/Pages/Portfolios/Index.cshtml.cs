using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Portfolios.GetAllPortfolios;
using MediatR;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Portfolios;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;

    public IndexModel(IMediator mediator, IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public IEnumerable<PortfolioDto> Portfolios { get; set; } = new List<PortfolioDto>();
    public SystemSettingsViewModel Settings { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Load settings
        Settings = await _settingsService.GetSettingsAsync();
        
        var result = await _mediator.Send(new GetAllPortfoliosRequest());
        
        if (result.IsSuccess)
        {
            Portfolios = result.Value;
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