using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.UpdateHistoryRecord;

public record UpdateHistoryRecordRequest : IRequest<Result>
{
    public int HistoryRecordId { get; init; }
    
    [Range(0, 999999999999, ErrorMessage = "Value must be between 0 and 999,999,999,999")]
    public decimal Value { get; init; }
    
    public string? Note { get; init; }
} 