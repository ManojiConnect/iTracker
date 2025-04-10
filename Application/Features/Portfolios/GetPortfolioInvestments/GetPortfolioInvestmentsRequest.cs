using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Features.Investments.Common;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.GetPortfolioInvestments;

public record GetPortfolioInvestmentsRequest : IRequest<Result<List<InvestmentDto>>>
{
    public required int PortfolioId { get; init; }
}

public class GetPortfolioInvestmentsHandler : IRequestHandler<GetPortfolioInvestmentsRequest, Result<List<InvestmentDto>>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioInvestmentsHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<InvestmentDto>>> Handle(GetPortfolioInvestmentsRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }
        
        var portfolio = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && !p.IsDelete && p.UserId == userId, cancellationToken);

        if (portfolio == null)
        {
            return Result.NotFound("Portfolio not found");
        }

        var investments = await _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .Where(i => i.PortfolioId == request.PortfolioId && !i.IsDelete)
            .Select(i => new InvestmentDto
            {
                Id = i.Id,
                Name = i.Name,
                Symbol = i.Symbol,
                TotalInvestment = i.TotalInvestment,
                CurrentValue = i.CurrentValue,
                UnrealizedGainLoss = i.UnrealizedGainLoss,
                ReturnPercentage = i.ReturnPercentage,
                PurchaseDate = i.PurchaseDate,
                PurchasePrice = i.PurchasePrice,
                Notes = i.Notes,
                PortfolioId = i.PortfolioId,
                PortfolioName = i.Portfolio.Name,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.Name
            })
            .ToListAsync(cancellationToken);

        return Result.Success(investments);
    }
} 