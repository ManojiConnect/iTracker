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
    private readonly ICurrentUserService _currentUserService;

    public CreatePortfolioHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }
        
        // Check if portfolio with the same name already exists for this user
        var existingPortfolio = await _context.Portfolios
            .AnyAsync(p => p.Name == request.Name && p.UserId == userId && !p.IsDelete, cancellationToken);
            
        if (existingPortfolio)
        {
            return Result.Error("A portfolio with this name already exists");
        }

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
            IsDelete = false,
            UserId = userId // Set the UserId to the current user's ID
        };

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(portfolio.Id);
    }
} 