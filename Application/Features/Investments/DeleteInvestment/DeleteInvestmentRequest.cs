using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Features.Investments.DeleteInvestment;

public record DeleteInvestmentRequest : IRequest<Result<int>>
{
    public int Id { get; init; }
}

public class DeleteInvestmentHandler : IRequestHandler<DeleteInvestmentRequest, Result<int>>
{
    private readonly IContext _context;

    public DeleteInvestmentHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(DeleteInvestmentRequest request, CancellationToken cancellationToken)
    {
        var investment = await _context.Investments
            .Include(i => i.Portfolio)
            .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDelete, cancellationToken);
        
        if (investment == null)
        {
            return Result.NotFound("Investment not found");
        }

        // Get reference to portfolio for recalculation
        var portfolio = investment.Portfolio;

        // Soft delete the investment
        investment.IsDelete = true;
        investment.ModifiedBy = 1;
        investment.ModifiedOn = DateTime.UtcNow;

        // Update portfolio totals
        if (portfolio != null)
        {
            // Recalculate portfolio totals based on all non-deleted investments
            portfolio.TotalValue = portfolio.Investments
                .Where(i => !i.IsDelete)
                .Sum(i => i.CurrentValue);
                
            portfolio.TotalInvestment = portfolio.Investments
                .Where(i => !i.IsDelete)
                .Sum(i => i.TotalInvestment);
                
            portfolio.UnrealizedGainLoss = portfolio.TotalValue - portfolio.TotalInvestment;
            portfolio.ReturnPercentage = portfolio.TotalInvestment > 0 
                ? (portfolio.UnrealizedGainLoss / portfolio.TotalInvestment) * 100 
                : 0;
            
            portfolio.ModifiedBy = 1;
            portfolio.ModifiedOn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(investment.Id);
    }
} 