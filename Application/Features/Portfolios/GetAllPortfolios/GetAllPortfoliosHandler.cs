using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
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


        var portfolios = await _context.Portfolios
            .AsNoTracking()
            .Include(p => p.Investments.Where(i => !i.IsDelete))
            .Where(p => !p.IsDelete && p.UserId == userId) // Only show portfolios for the current user
            .OrderBy(p => p.Name)
            .Select(p => new PortfolioDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                TotalValue = p.TotalValue,
                ReturnPercentage = p.ReturnPercentage,
                // Additional properties
                InvestmentCount = p.Investments.Count(i => !i.IsDelete),
                TotalInvested = p.TotalInvestment,
                Performance = p.TotalInvestment > 0 ? p.UnrealizedGainLoss / p.TotalInvestment : 0,
                CreatedOn = p.CreatedOn
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<PortfolioDto>>(portfolios);
    }
} 