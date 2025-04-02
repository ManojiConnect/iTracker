using Application.Features.InvestmentCategories.CreateCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.InvestmentCategories;

public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public CreateCategoryRequest Category { get; set; } = new() { Name = string.Empty };

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

        var result = await _mediator.Send(Category);
        
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