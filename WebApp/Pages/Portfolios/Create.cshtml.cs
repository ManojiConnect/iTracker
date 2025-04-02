using Application.Features.Portfolios.CreatePortfolio;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Portfolios;

public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public CreatePortfolioRequest Portfolio { get; set; } = new()
    {
        Name = string.Empty,
        Description = string.Empty,
        InitialValue = 0
    };

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(Portfolio);
        
        if (result.IsSuccess)
        {
            return RedirectToPage("./Details", new { id = result.Value });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return Page();
    }
} 