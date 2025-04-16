using Ardalis.Result;
using MediatR;

namespace Application.Features.Investments.DeleteHistoryRecord;

public record DeleteHistoryRecordRequest : IRequest<Result>
{
    public int HistoryRecordId { get; init; }
} 