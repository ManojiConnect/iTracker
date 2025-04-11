using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Features.Common.Responses;
using Application.Features.Investments.Common;
using Domain.Entities;
using Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Abstractions.Services;

namespace Application.Features.Investments.GetAllInvestments;

public class GetAllInvestmentsHandler : IRequestHandler<GetAllInvestmentsRequest, Result<PaginatedList<InvestmentDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetAllInvestmentsHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetAllInvestmentsHandler(
        AppDbContext dbContext, 
        ILogger<GetAllInvestmentsHandler> logger,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<InvestmentDto>>> Handle(GetAllInvestmentsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current user ID
            var userId = _currentUserService.Id;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Attempt to access investments without valid user ID");
                return Result<PaginatedList<InvestmentDto>>.Failure("Unauthorized access");
            }

            // Start with a query that joins all the needed entities and filters by user
            var query = _dbContext.Investments
                .Include(i => i.Portfolio)
                .Include(i => i.Category)
                .Where(i => !i.IsDelete && i.Portfolio.UserId == userId);

            // Apply portfolio filter
            if (request.PortfolioId.HasValue)
            {
                query = query.Where(i => i.PortfolioId == request.PortfolioId.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchTerm = request.SearchText.ToLower();
                query = query.Where(i => 
                    i.Name.ToLower().Contains(searchTerm) || 
                    i.Symbol.ToLower().Contains(searchTerm) || 
                    i.Portfolio.Name.ToLower().Contains(searchTerm) || 
                    i.Category.Name.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(i => i.Category.Name == request.Category);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortOrder);

            // Apply pagination
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new InvestmentDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Symbol = i.Symbol,
                    TotalInvestment = i.TotalInvestment,
                    CurrentValue = i.CurrentValue,
                    UnrealizedGainLoss = i.UnrealizedGainLoss,
                    ReturnPercentage = i.ReturnPercentage,
                    PurchaseDate = i.PurchaseDate,
                    PurchasePrice = i.PurchasePrice,
                    Notes = i.Notes,
                    PortfolioId = i.PortfolioId,
                    PortfolioName = i.Portfolio.Name,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category.Name
                })
                .ToListAsync(cancellationToken);

            // Create paginated response
            var result = new PaginatedList<InvestmentDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<InvestmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving investments");
            return Result<PaginatedList<InvestmentDto>>.Failure($"Error retrieving investments: {ex.Message}");
        }
    }

    private static IQueryable<Investment> ApplySorting(IQueryable<Investment> query, string sortBy, string sortOrder)
    {
        var isAscending = string.IsNullOrEmpty(sortOrder) || sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "name" => isAscending 
                ? query.OrderBy(i => i.Name) 
                : query.OrderByDescending(i => i.Name),
                
            "category" => isAscending 
                ? query.OrderBy(i => i.Category.Name) 
                : query.OrderByDescending(i => i.Category.Name),
                
            "portfolio" => isAscending 
                ? query.OrderBy(i => i.Portfolio.Name) 
                : query.OrderByDescending(i => i.Portfolio.Name),
                
            "initialinvestment" => isAscending 
                ? query.OrderBy(i => i.TotalInvestment) 
                : query.OrderByDescending(i => i.TotalInvestment),
                
            "currentvalue" => isAscending 
                ? query.OrderBy(i => i.CurrentValue) 
                : query.OrderByDescending(i => i.CurrentValue),
                
            "return" => isAscending 
                ? query.OrderBy(i => i.ReturnPercentage) 
                : query.OrderByDescending(i => i.ReturnPercentage),
                
            _ => isAscending 
                ? query.OrderBy(i => i.Name) 
                : query.OrderByDescending(i => i.Name)
        };
    }
} 