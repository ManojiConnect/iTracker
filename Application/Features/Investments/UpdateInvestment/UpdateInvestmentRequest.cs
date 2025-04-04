using System;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.UpdateInvestment;

public record UpdateInvestmentRequest : IRequest<Result<int>>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int CategoryId { get; init; }
    public required decimal TotalInvestment { get; init; }
    public required decimal CurrentValue { get; init; }
    public required DateTime PurchaseDate { get; init; }
} 