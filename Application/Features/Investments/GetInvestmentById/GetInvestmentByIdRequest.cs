using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Features.Investments.Common;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetInvestmentById;

public record GetInvestmentByIdRequest : IRequest<Result<InvestmentDto>>
{
    public required int Id { get; init; }
}

public class GetInvestmentByIdHandler : IRequestHandler<GetInvestmentByIdRequest, Result<InvestmentDto>>
{
    private readonly IContext _context;

    public GetInvestmentByIdHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<InvestmentDto>> Handle(GetInvestmentByIdRequest request, CancellationToken cancellationToken)
    {
        var investment = await _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDelete, cancellationToken);

        if (investment == null)
        {
            return Result.NotFound();
        }

        // Recalculate return percentage
        var returnPercentage = investment.TotalInvestment > 0 
            ? (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100 
            : 0;

        var response = new InvestmentDto
        {
            Id = investment.Id,
            Name = investment.Name,
            Symbol = investment.Symbol,
            TotalInvestment = investment.TotalInvestment,
            CurrentValue = investment.CurrentValue,
            UnrealizedGainLoss = investment.UnrealizedGainLoss,
            ReturnPercentage = returnPercentage,
            PurchaseDate = investment.PurchaseDate,
            PurchasePrice = investment.PurchasePrice,
            Notes = investment.Notes,
            PortfolioId = investment.PortfolioId,
            PortfolioName = investment.Portfolio.Name,
            CategoryId = investment.CategoryId,
            CategoryName = investment.Category.Name
        };

        return Result.Success(response);
    }
} 