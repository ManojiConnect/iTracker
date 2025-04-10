using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Common.Models;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetAllInvestments;

public class GetAllInvestmentsHandler : IRequestHandler<GetAllInvestmentsRequest, Result<PaginatedList<InvestmentDto>>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllInvestmentsHandler(IContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<InvestmentDto>>> Handle(GetAllInvestmentsRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Result<PaginatedList<InvestmentDto>>.Failure("Unauthorized: User must be logged in");
        }
        
        // First get all portfolios for the current user
        var userPortfolioIds = await _context.Portfolios
            .Where(p => p.UserId == userId && !p.IsDelete)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
            
        // Then get investments from those portfolios with pagination
        var query = _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .Where(i => !i.IsDelete && userPortfolioIds.Contains(i.PortfolioId))
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
            });

        var paginatedList = await PaginatedList<InvestmentDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<InvestmentDto>>.Success(paginatedList);
    }
} 