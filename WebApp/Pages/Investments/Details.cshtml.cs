using System;
using System.Threading.Tasks;
using Application.Features.Investments.GetInvestmentById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.Investments;

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DetailsModel> _logger;

    public InvestmentResponse Investment { get; set; } = default!;

    public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
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
} 