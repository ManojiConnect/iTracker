using Application.Features.InvestmentCategories.GetCategoryById;
using Application.Features.InvestmentCategories.DeleteCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.InvestmentCategories;

public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public CategoryDto Category { get; set; } = new() { Name = string.Empty };

    public DeleteModel(IMediator mediator)
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

        Category = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear existing model state to prevent unnecessary validation errors
        ModelState.Clear();
        
        var result = await _mediator.Send(new DeleteCategoryRequest { Id = Category.Id });
        
        if (result.IsSuccess)
        {
            return RedirectToPage("./Index");
        }

        // Get the category details again to ensure we display correct info
        var categoryResult = await _mediator.Send(new GetCategoryByIdRequest { Id = Category.Id });
        if (categoryResult.IsSuccess)
        {
            Category = categoryResult.Value;
        }

        // Add only the actual error message about investments
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("DeleteError", error);
        }

        return Page();
    }
} 