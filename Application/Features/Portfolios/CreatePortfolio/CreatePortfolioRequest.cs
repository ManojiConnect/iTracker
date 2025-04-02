using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.CreatePortfolio;

public record CreatePortfolioRequest : IRequest<Result<int>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal InitialValue { get; init; }
}

public class CreatePortfolioHandler : IRequestHandler<CreatePortfolioRequest, Result<int>>
{
    private readonly IContext _context;

    public CreatePortfolioHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var portfolio = new Domain.Entities.Portfolio
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            InitialValue = request.InitialValue,
            TotalValue = request.InitialValue,
            TotalInvestment = request.InitialValue,
            UnrealizedGainLoss = 0,
            ReturnPercentage = 0,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = 1,
            ModifiedOn = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false
        };

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(portfolio.Id);
    }
} 