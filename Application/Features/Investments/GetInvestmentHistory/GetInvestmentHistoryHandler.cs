using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetInvestmentHistory;

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