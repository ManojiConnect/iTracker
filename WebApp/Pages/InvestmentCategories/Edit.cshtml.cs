using Application.Features.InvestmentCategories.GetCategoryById;
using Application.Features.InvestmentCategories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.InvestmentCategories;

public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public UpdateCategoryRequest Category { get; set; } = new() { Name = string.Empty };

    public EditModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdRequest { Id = id });
        
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var category = result.Value;
        Category = new UpdateCategoryRequest
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

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