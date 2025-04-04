using System;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.RecordValueUpdate;

public record RecordValueUpdateRequest : IRequest<Result<int>>
{
    public int InvestmentId { get; init; }
    public decimal NewValue { get; init; }
    public string? Note { get; init; }
} 