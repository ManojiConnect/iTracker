using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetAllInvestments;

public class GetAllInvestmentsHandler : IRequestHandler<GetAllInvestmentsRequest, Result<List<InvestmentDto>>>
{
    private readonly IContext _context;

    public GetAllInvestmentsHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<List<InvestmentDto>>> Handle(GetAllInvestmentsRequest request, CancellationToken cancellationToken)
    {
        var investments = await _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .Where(i => !i.IsDelete)
            .Select(i => new InvestmentDto
            {
                Id = i.Id,
                Name = i.Name,
                TotalInvestment = i.TotalInvestment,
                CurrentValue = i.CurrentValue,
                UnrealizedGainLoss = i.UnrealizedGainLoss,
                ReturnPercentage = i.ReturnPercentage,
                PurchaseDate = i.PurchaseDate,
                CategoryName = i.Category.Name,
                PortfolioName = i.Portfolio.Name,
                Category = i.Category,
                Portfolio = i.Portfolio
            })
            .ToListAsync(cancellationToken);

        return Result<List<InvestmentDto>>.Success(investments);
    }
} 