using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.RecordValueUpdate;

public class RecordValueUpdateHandler : IRequestHandler<RecordValueUpdateRequest, Result<int>>
{
    private readonly IContext _context;

    public RecordValueUpdateHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(RecordValueUpdateRequest request, CancellationToken cancellationToken)
    {
        // Find the investment
        var investment = await _context.Investments
            .Include(i => i.Portfolio)
            .FirstOrDefaultAsync(i => i.Id == request.InvestmentId && !i.IsDelete, cancellationToken);

        if (investment == null)
        {
            return Result.NotFound("Investment not found");
        }

        // Create a new history record
        var history = new InvestmentHistory
        {
            InvestmentId = investment.Id,
            Value = request.NewValue,
            RecordedDate = DateTime.UtcNow,
            Note = request.Note
        };

        // Update the investment's current value
        decimal oldValue = investment.CurrentValue;
        investment.CurrentValue = request.NewValue;
        
        // Recalculate gains and performance
        investment.UnrealizedGainLoss = investment.CurrentValue - investment.TotalInvestment;
        investment.ReturnPercentage = investment.TotalInvestment > 0 
            ? (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100 
            : 0;

        // Update the portfolio totals
        if (investment.Portfolio != null)
        {
            investment.Portfolio.TotalValue = investment.Portfolio.TotalValue - oldValue + request.NewValue;
            investment.Portfolio.UnrealizedGainLoss = investment.Portfolio.TotalValue - investment.Portfolio.TotalInvestment;
            investment.Portfolio.ReturnPercentage = investment.Portfolio.TotalInvestment > 0
                ? (investment.Portfolio.UnrealizedGainLoss / investment.Portfolio.TotalInvestment) * 100
                : 0;
        }

        // Save the changes
        _context.InvestmentHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(history.Id);
    }
} 