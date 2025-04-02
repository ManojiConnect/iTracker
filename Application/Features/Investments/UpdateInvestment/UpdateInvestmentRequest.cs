using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.UpdateInvestment;

public record UpdateInvestmentRequest : IRequest<Result<int>>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int CategoryId { get; init; }
    public required decimal TotalInvestment { get; init; }
    public required decimal CurrentValue { get; init; }
    public required DateTime PurchaseDate { get; init; }
}

public class UpdateInvestmentHandler : IRequestHandler<UpdateInvestmentRequest, Result<int>>
{
    private readonly IContext _context;

    public UpdateInvestmentHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdateInvestmentRequest request, CancellationToken cancellationToken)
    {
        // Get investment with portfolio
        var investment = await _context.Investments
            .Include(i => i.Portfolio)
            .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDelete, cancellationToken);

        if (investment == null)
        {
            return Result.NotFound("Investment not found");
        }

        // Validate category exists
        var category = await _context.InvestmentCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete, cancellationToken);

        if (category == null)
        {
            return Result.NotFound("Investment category not found");
        }

        // Update investment
        investment.Name = request.Name;
        investment.CategoryId = request.CategoryId;
        investment.TotalInvestment = request.TotalInvestment;
        investment.CurrentValue = request.CurrentValue;
        investment.PurchaseDate = request.PurchaseDate;
        investment.ModifiedBy = 1;
        investment.ModifiedOn = DateTime.UtcNow;

        // Update portfolio totals
        var portfolio = investment.Portfolio;
        portfolio.TotalValue = portfolio.Investments.Where(i => !i.IsDelete).Sum(i => i.CurrentValue);
        portfolio.TotalInvestment = portfolio.Investments.Where(i => !i.IsDelete).Sum(i => i.TotalInvestment);
        portfolio.UnrealizedGainLoss = portfolio.TotalValue - portfolio.TotalInvestment;
        portfolio.ReturnPercentage = portfolio.TotalInvestment > 0 
            ? (portfolio.UnrealizedGainLoss / portfolio.TotalInvestment) * 100 
            : 0;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(investment.Id);
    }
} 