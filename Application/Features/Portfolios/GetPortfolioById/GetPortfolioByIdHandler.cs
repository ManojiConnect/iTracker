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

        return Result.Success(new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            InitialValue = portfolio.InitialValue,
            TotalValue = portfolio.TotalValue,
            TotalInvestment = portfolio.TotalInvestment,
            UnrealizedGainLoss = portfolio.UnrealizedGainLoss,
            ReturnPercentage = portfolio.ReturnPercentage,
            CreatedOn = portfolio.CreatedOn,
            CreatedBy = portfolio.CreatedBy,
            ModifiedOn = portfolio.ModifiedOn,
            ModifiedBy = portfolio.ModifiedBy,
            IsActive = portfolio.IsActive,
            IsDelete = portfolio.IsDelete
        });
    }
} 