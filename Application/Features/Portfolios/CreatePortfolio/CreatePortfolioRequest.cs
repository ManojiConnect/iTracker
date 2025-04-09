using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.CreatePortfolio;

public record CreatePortfolioRequest : IRequest<Result<int>>
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public required string Name { get; init; }
    
    [StringLength(500)]
    public string? Description { get; init; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Initial value must be greater than or equal to 0")]
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

        // Validate required fields
        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Error("Name is required");
        }

        // Validate name length
        if (request.Name.Length > 100)
        {
            return Result.Error("Name cannot exceed 100 characters");
        }
        
        // Check if portfolio with the same name already exists for this user
        var existingPortfolio = await _context.Portfolios
            .AnyAsync(p => p.Name == request.Name && p.UserId == userId && !p.IsDelete, cancellationToken);
            
        if (existingPortfolio)
        {
            return Result.Error("Portfolio with this name already exists");
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
            UserId = userId
        };

        try
        {
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(portfolio.Id);
        }
        catch (Exception)
        {
            return Result.Error("Failed to create portfolio");
        }
    }
} 