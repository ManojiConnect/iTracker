using Application.Features.Investments.DeleteInvestment;
using Application.Features.Investments.GetInvestmentById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Investments;

public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;

    public InvestmentResponse Investment { get; set; } = null!;

    public DeleteModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id.Value });
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        Investment = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new DeleteInvestmentRequest { Id = id.Value });
        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            
            // Reload investment data for the view
            var investmentResult = await _mediator.Send(new GetInvestmentByIdRequest { Id = id.Value });
            if (investmentResult.IsSuccess)
            {
                Investment = investmentResult.Value;
            }
            
            return Page();
        }

        // Redirect to the investments index page
        return RedirectToPage("./Index");
    }
} 