using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetAllInvestments;

public class GetAllInvestmentsHandler : IRequestHandler<GetAllInvestmentsRequest, Result<List<InvestmentDto>>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllInvestmentsHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<InvestmentDto>>> Handle(GetAllInvestmentsRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result<List<InvestmentDto>>.Failure("Unauthorized: User must be logged in");
        }
        
        // First get all portfolios for the current user
        var userPortfolioIds = await _context.Portfolios
            .Where(p => p.UserId == userId && !p.IsDelete)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
            
        // Then get investments from those portfolios
        var investments = await _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .Where(i => !i.IsDelete && userPortfolioIds.Contains(i.PortfolioId))
            .Select(i => new InvestmentDto
            {
                Id = i.Id,
                Name = i.Name,
                TotalInvestment = i.TotalInvestment,
                CurrentValue = i.CurrentValue,
                UnrealizedGainLoss = i.UnrealizedGainLoss,
                ReturnPercentage = i.ReturnPercentage,
                PurchaseDate = i.PurchaseDate,
                CategoryName = i.Category.Name,
                PortfolioName = i.Portfolio.Name,
                Category = i.Category,
                Portfolio = i.Portfolio
            })
            .ToListAsync(cancellationToken);

        return Result<List<InvestmentDto>>.Success(investments);
    }
} 