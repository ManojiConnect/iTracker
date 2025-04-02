using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Portfolios.GetAllPortfolios;
using MediatR;

namespace WebApp.Pages.Portfolios;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IEnumerable<PortfolioDto> Portfolios { get; set; } = new List<PortfolioDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetAllPortfoliosRequest());
        
        if (result.IsSuccess)
        {
            Portfolios = result.Value;
        }

        return Page();
    }
} 