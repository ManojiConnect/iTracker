using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.GetInvestmentHistory;

public record GetInvestmentHistoryRequest : IRequest<Result<List<InvestmentHistoryDto>>>
{
    public int InvestmentId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record InvestmentHistoryDto
{
    public int Id { get; init; }
    public int InvestmentId { get; init; }
    public string InvestmentName { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public DateTime RecordedDate { get; init; }
    public string? Note { get; init; }
} 