using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.GetPortfolioInvestments;

public record GetPortfolioInvestmentsRequest : IRequest<Result<List<InvestmentResponse>>>
{
    public required int PortfolioId { get; init; }
}

public class InvestmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class GetPortfolioInvestmentsHandler : IRequestHandler<GetPortfolioInvestmentsRequest, Result<List<InvestmentResponse>>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioInvestmentsHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<InvestmentResponse>>> Handle(GetPortfolioInvestmentsRequest request, CancellationToken cancellationToken)
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
            .Where(i => i.PortfolioId == request.PortfolioId && !i.IsDelete)
            .Select(i => new InvestmentResponse
            {
                Id = i.Id,
                Name = i.Name,
                TotalInvestment = i.TotalInvestment,
                CurrentValue = i.CurrentValue,
                UnrealizedGainLoss = i.CurrentValue - i.TotalInvestment,
                ReturnPercentage = i.TotalInvestment > 0 ? (i.CurrentValue - i.TotalInvestment) / i.TotalInvestment : 0,
                PurchaseDate = i.PurchaseDate,
                CategoryName = i.Category.Name
            })
            .ToListAsync(cancellationToken);

        return Result.Success(investments);
    }
} 