using Application.Features.InvestmentCategories.GetAllCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.InvestmentCategories;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetAllCategoriesRequest());
        
        if (result.IsSuccess)
        {
            Categories = result.Value;
        }

        return Page();
    }
} 