using System;
using System.Threading.Tasks;
using Application.Features.Investments.GetInvestmentById;
using Application.Features.Investments.DeleteInvestment;
using Application.Features.Investments.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebApp.Services;
using WebApp.Models;
using System.Linq;

namespace WebApp.Pages.Investments;

public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteModel> _logger;
    private readonly IApplicationSettingsService _settingsService;

    public InvestmentDto Investment { get; set; } = null!;
    public SystemSettingsViewModel Settings { get; set; }

    public DeleteModel(
        IMediator mediator,
        ILogger<DeleteModel> logger,
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
        
        _logger.LogInformation("Fetching investment details for deletion, ID: {Id}", id);
        
        var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id });
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Investment with ID {Id} not found", id);
            return NotFound();
        }

        Investment = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        _logger.LogInformation("Deleting investment with ID: {Id}", id);
        
        var result = await _mediator.Send(new DeleteInvestmentRequest { Id = id });
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete investment with ID {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully deleted investment with ID: {Id}", id);
        return RedirectToPage("./Index");
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