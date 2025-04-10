using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Mapster;
using System.Linq;
using System;

namespace Application.Features.Portfolios.GetAllPortfolios;

public class GetAllPortfoliosHandler : IRequestHandler<GetAllPortfoliosRequest, Result<IEnumerable<PortfolioDto>>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllPortfoliosHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<PortfolioDto>>> Handle(GetAllPortfoliosRequest request, CancellationToken cancellationToken)
    {
        // Get the current user ID
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }

        // First get portfolios with their investments
        var portfolios = await _context.Portfolios
            .AsNoTracking()
            .Include(p => p.Investments.Where(i => !i.IsDelete))
            .Where(p => !p.IsDelete && p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        // Then calculate the DTOs in memory
        var portfolioDtos = portfolios.Select(p => new PortfolioDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            TotalValue = p.Investments.Sum(i => i.CurrentValue),
            ReturnPercentage = p.Investments.Sum(i => i.TotalInvestment) > 0 
                ? (p.Investments.Sum(i => i.CurrentValue - i.TotalInvestment) / p.Investments.Sum(i => i.TotalInvestment)) * 100
                : 0,
            InvestmentCount = p.Investments.Count,
            TotalInvested = p.Investments.Sum(i => i.TotalInvestment),
            Performance = p.Investments.Sum(i => i.TotalInvestment) > 0 
                ? (p.Investments.Sum(i => i.CurrentValue - i.TotalInvestment) / p.Investments.Sum(i => i.TotalInvestment)) * 100
                : 0,
            CreatedOn = p.CreatedOn
        }).ToList();

        return Result.Success<IEnumerable<PortfolioDto>>(portfolioDtos);
    }
} 