using Ardalis.Result;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Portfolios.GetAllPortfolios;

public record GetAllPortfoliosRequest : IRequest<Result<IEnumerable<PortfolioDto>>>;

public record PortfolioDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal TotalValue { get; init; }
    public decimal ReturnPercentage { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public DateTime? ModifiedOn { get; init; }
    
    // Additional properties needed for views
    public int InvestmentCount { get; init; }
    public decimal TotalInvested { get; init; }
    public decimal Performance { get; init; }
    public DateTime CreatedOn { get; init; }
} 