using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolios.GetPortfolioById;

public record GetPortfolioByIdRequest : IRequest<Result<PortfolioDto>>
{
    public required int Id { get; init; }
}

public record PortfolioDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal InitialValue { get; init; }
    public decimal TotalValue { get; init; }
    public decimal TotalInvestment { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal ReturnPercentage { get; init; }
    public DateTime CreatedOn { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public int? ModifiedBy { get; init; }
    public bool IsActive { get; init; }
    public bool IsDelete { get; init; }
} 