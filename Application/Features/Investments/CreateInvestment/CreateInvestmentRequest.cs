using System;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.CreateInvestment;

public record CreateInvestmentRequest : IRequest<Result<int>>
{
    public required int PortfolioId { get; init; }
    public required string Name { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public required int CategoryId { get; init; }
    public required decimal TotalInvestment { get; init; }
    public required decimal CurrentValue { get; init; }
    public required DateTime PurchaseDate { get; init; }
    public decimal PurchasePrice { get; init; }
    public string? Notes { get; init; }
} 