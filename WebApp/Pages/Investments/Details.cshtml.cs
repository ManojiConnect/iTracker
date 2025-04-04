using System;
using System.Threading.Tasks;
using Application.Features.Investments.GetInvestmentById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Investments;

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DetailsModel> _logger;
    private readonly IApplicationSettingsService _settingsService;

    public InvestmentResponse Investment { get; set; } = default!;
    public SystemSettingsViewModel Settings { get; set; }

    public DetailsModel(
        IMediator mediator, 
        ILogger<DetailsModel> logger,
        IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _logger = logger;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Load settings
        Settings = await _settingsService.GetSettingsAsync();
        
        _logger.LogInformation("Fetching investment details for ID: {Id}", id);
        
        var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id });
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Investment with ID {Id} not found", id);
            return NotFound();
        }

        Investment = result.Value;
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
    
    public string FormatDate(DateTime date)
    {
        return _settingsService.FormatDate(date);
    }
} 