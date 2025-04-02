using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

public class GetInvestmentHistoryHandler : IRequestHandler<GetInvestmentHistoryRequest, Result<List<InvestmentHistoryDto>>>
{
    private readonly IContext _context;

    public GetInvestmentHistoryHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<List<InvestmentHistoryDto>>> Handle(GetInvestmentHistoryRequest request, CancellationToken cancellationToken)
    {
        var query = _context.InvestmentHistories
            .Include(h => h.Investment)
            .Where(h => h.InvestmentId == request.InvestmentId);

        if (request.StartDate.HasValue)
        {
            query = query.Where(h => h.RecordedDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(h => h.RecordedDate <= request.EndDate.Value);
        }

        var history = await query
            .OrderBy(h => h.RecordedDate)
            .Select(h => new InvestmentHistoryDto
            {
                Id = h.Id,
                InvestmentId = h.InvestmentId,
                InvestmentName = h.Investment.Name,
                Value = h.Value,
                RecordedDate = h.RecordedDate,
                Note = h.Note
            })
            .ToListAsync(cancellationToken);

        return Result.Success(history);
    }
} 