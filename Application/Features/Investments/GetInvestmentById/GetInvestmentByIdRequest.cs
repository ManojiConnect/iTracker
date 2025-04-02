using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Features.Investments.GetAllInvestments;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetInvestmentById;

public record GetInvestmentByIdRequest : IRequest<Result<InvestmentResponse>>
{
    public required int Id { get; init; }
}

public class InvestmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string PortfolioName { get; set; } = string.Empty;
}

public class GetInvestmentByIdHandler : IRequestHandler<GetInvestmentByIdRequest, Result<InvestmentResponse>>
{
    private readonly IContext _context;

    public GetInvestmentByIdHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<InvestmentResponse>> Handle(GetInvestmentByIdRequest request, CancellationToken cancellationToken)
    {
        var investment = await _context.Investments
            .Include(i => i.Category)
            .Include(i => i.Portfolio)
            .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDelete, cancellationToken);

        if (investment == null)
        {
            return Result.NotFound();
        }

        var response = new InvestmentResponse
        {
            Id = investment.Id,
            Name = investment.Name,
            TotalInvestment = investment.TotalInvestment,
            CurrentValue = investment.CurrentValue,
            UnrealizedGainLoss = investment.UnrealizedGainLoss,
            ReturnPercentage = investment.ReturnPercentage,
            PurchaseDate = investment.PurchaseDate,
            CategoryName = investment.Category.Name,
            PortfolioName = investment.Portfolio.Name
        };

        return Result.Success(response);
    }
}

// The local InvestmentDto class to avoid conflicts
public class InvestmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string PortfolioName { get; set; } = string.Empty;
    
    // Navigation properties for views
    public InvestmentCategory? Category { get; set; }
    public Portfolio? Portfolio { get; set; }
} 