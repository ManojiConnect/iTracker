using Application.Features.Investments.GetAllInvestments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Investments;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;
    
    public List<InvestmentDto> Investments { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; }

    public IndexModel(IMediator mediator, IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Load settings
        Settings = await _settingsService.GetSettingsAsync();
        
        var result = await _mediator.Send(new GetAllInvestmentsRequest());
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to load investments");
            return Page();
        }

        Investments = result.Value;
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