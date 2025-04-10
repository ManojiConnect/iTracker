using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.GetPortfolioById;

public class GetPortfolioByIdHandler : IRequestHandler<GetPortfolioByIdRequest, Result<PortfolioDto>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioByIdHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PortfolioDto>> Handle(GetPortfolioByIdRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }
        
        var portfolio = await _context.Portfolios
            .Include(p => p.Investments.Where(i => !i.IsDelete))
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDelete && p.UserId == userId, cancellationToken);

        if (portfolio == null)
        {
            return Result.NotFound("Portfolio not found");
        }

        // Recalculate totals based on investments
        var totalValue = portfolio.Investments.Sum(i => i.CurrentValue);
        var totalInvestment = portfolio.Investments.Sum(i => i.TotalInvestment);
        var unrealizedGainLoss = totalValue - totalInvestment;
        var returnPercentage = totalInvestment > 0 
            ? (unrealizedGainLoss / totalInvestment) * 100
            : 0;

        return Result.Success(new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            InitialValue = portfolio.InitialValue,
            TotalValue = totalValue,
            TotalInvestment = totalInvestment,
            UnrealizedGainLoss = unrealizedGainLoss,
            ReturnPercentage = returnPercentage,
            CreatedOn = portfolio.CreatedOn,
            CreatedBy = portfolio.CreatedBy,
            ModifiedOn = portfolio.ModifiedOn,
            ModifiedBy = portfolio.ModifiedBy,
            IsActive = portfolio.IsActive,
            IsDelete = portfolio.IsDelete
        });
    }
} 