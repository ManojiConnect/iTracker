using Application.Features.Investments.GetAllInvestments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Investments;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public List<InvestmentDto> Investments { get; set; } = new();

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetAllInvestmentsRequest());
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to load investments");
            return Page();
        }

        Investments = result.Value;
        return Page();
    }
} 