using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Pages.Portfolios;

public class CategoryDistributionItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
}

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;

    public PortfolioDto Portfolio { get; set; } = null!;
    public IEnumerable<InvestmentResponse> Investments { get; set; } = new List<InvestmentResponse>();
    public List<CategoryDistributionItem> CategoryDistribution { get; set; } = new();

    public DetailsModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var portfolioResult = await _mediator.Send(new GetPortfolioByIdRequest { Id = id });
        
        if (!portfolioResult.IsSuccess)
        {
            return NotFound();
        }

        Portfolio = portfolioResult.Value;

        var investmentsResult = await _mediator.Send(new GetPortfolioInvestmentsRequest { PortfolioId = id });
        if (investmentsResult.IsSuccess)
        {
            Investments = investmentsResult.Value;
            
            // Calculate category distribution
            if (Investments.Any())
            {
                var totalValue = Investments.Sum(i => i.CurrentValue);
                
                CategoryDistribution = Investments
                    .GroupBy(i => i.CategoryName ?? "Uncategorized")
                    .Select(g => new CategoryDistributionItem 
                    {
                        Name = g.Key,
                        Value = g.Sum(i => i.CurrentValue),
                        Percentage = totalValue > 0 ? (g.Sum(i => i.CurrentValue) / totalValue) * 100 : 0
                    })
                    .OrderByDescending(i => i.Value)
                    .ToList();
            }
        }

        return Page();
    }
} 