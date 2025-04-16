using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.RecordValueUpdate;

public record RecordValueUpdateRequest : IRequest<Result<int>>
{
    public int InvestmentId { get; init; }
    
    [Range(0, 999999999999, ErrorMessage = "Value must be between 0 and 999,999,999,999")]
    public decimal NewValue { get; init; }
    
    public string? Note { get; init; }
} 