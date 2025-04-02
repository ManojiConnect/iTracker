using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.GetPortfolioById;

public record GetPortfolioByIdRequest : IRequest<Result<PortfolioDto>>
{
    public required int Id { get; init; }
}

public record PortfolioDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal InitialValue { get; init; }
    public decimal TotalValue { get; init; }
    public decimal TotalInvestment { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal ReturnPercentage { get; init; }
    public DateTime CreatedOn { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public int? ModifiedBy { get; init; }
    public bool IsActive { get; init; }
    public bool IsDelete { get; init; }
}

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