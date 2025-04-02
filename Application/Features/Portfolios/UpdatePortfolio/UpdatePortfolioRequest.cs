using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.UpdatePortfolio;

public record UpdatePortfolioRequest : IRequest<Result<int>>
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal InitialValue { get; init; }
}

public class UpdatePortfolioHandler : IRequestHandler<UpdatePortfolioRequest, Result<int>>
{
    private readonly IContext _context;

    public UpdatePortfolioHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var portfolio = await _context.Portfolios
            .Include(p => p.Investments.Where(i => !i.IsDelete))
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDelete, cancellationToken);

        if (portfolio == null)
        {
            return Result.NotFound("Portfolio not found");
        }

        portfolio.Name = request.Name;
        portfolio.Description = request.Description;
        portfolio.InitialValue = request.InitialValue;
        portfolio.ModifiedBy = 1;
        portfolio.ModifiedOn = DateTime.UtcNow;

        // Recalculate totals
        portfolio.TotalValue = portfolio.Investments.Sum(i => i.CurrentValue);
        portfolio.TotalInvestment = portfolio.Investments.Sum(i => i.TotalInvestment);
        portfolio.UnrealizedGainLoss = portfolio.TotalValue - portfolio.TotalInvestment;
        portfolio.ReturnPercentage = portfolio.TotalInvestment > 0 
            ? (portfolio.UnrealizedGainLoss / portfolio.TotalInvestment) * 100 
            : 0;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(portfolio.Id);
    }
} 