using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.DeletePortfolio;
using Application.Features.Portfolios.GetPortfolioById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Portfolios;

public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    public PortfolioDto Portfolio { get; set; } = default!;

    public DeleteModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _mediator.Send(new GetPortfolioByIdRequest { Id = id });
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        Portfolio = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _mediator.Send(new DeletePortfolioRequest { Id = id });
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to delete portfolio");
            return Page();
        }

        return RedirectToPage("./Index");
    }
} 