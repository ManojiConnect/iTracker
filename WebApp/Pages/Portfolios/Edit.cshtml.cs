using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.UpdatePortfolio;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Portfolios;

public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public UpdatePortfolioRequest Portfolio { get; set; } = new()
    {
        Id = 0,
        Name = string.Empty,
        Description = string.Empty,
        InitialValue = 0
    };

    public EditModel(IMediator mediator)
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

        var portfolio = result.Value;
        Portfolio = new UpdatePortfolioRequest
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            InitialValue = portfolio.InitialValue
        };

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
            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return Page();
    }
} 