using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Investments.DeleteHistoryRecord;

public class DeleteHistoryRecordHandler : IRequestHandler<DeleteHistoryRecordRequest, Result>
{
    private readonly IContext _context;
    private readonly ILogger<DeleteHistoryRecordHandler> _logger;

    public DeleteHistoryRecordHandler(IContext context, ILogger<DeleteHistoryRecordHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteHistoryRecordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the history record to delete
            var historyRecord = await _context.InvestmentHistories
                .Include(h => h.Investment)
                .ThenInclude(i => i.Portfolio)
                .FirstOrDefaultAsync(h => h.Id == request.HistoryRecordId, cancellationToken);

            if (historyRecord == null)
            {
                _logger.LogWarning("History record with ID {RecordId} not found", request.HistoryRecordId);
                return Result.NotFound("History record not found");
            }

            var investmentId = historyRecord.InvestmentId;
            var recordValue = historyRecord.Value;
            var recordDate = historyRecord.RecordedDate;
            
            // Check if this is the only history record for the investment
            var isOnlyRecord = await _context.InvestmentHistories
                .CountAsync(h => h.InvestmentId == investmentId, cancellationToken) == 1;

            if (isOnlyRecord)
            {
                _logger.LogWarning("Cannot delete the only history record for investment {InvestmentId}", investmentId);
                return Result.Error("Cannot delete the only history record for this investment");
            }

            // Check if this is the most recent record
            var isMostRecent = await _context.InvestmentHistories
                .Where(h => h.InvestmentId == investmentId)
                .OrderByDescending(h => h.RecordedDate)
                .Select(h => h.Id)
                .FirstOrDefaultAsync(cancellationToken) == historyRecord.Id;

            // Remove the history record
            _context.InvestmentHistories.Remove(historyRecord);

            // If this was the most recent record, update the investment to use the next most recent record
            if (isMostRecent && historyRecord.Investment != null)
            {
                var investment = historyRecord.Investment;
                
                // Get the new most recent record
                var newMostRecent = await _context.InvestmentHistories
                    .Where(h => h.InvestmentId == investmentId && h.Id != request.HistoryRecordId)
                    .OrderByDescending(h => h.RecordedDate)
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (newMostRecent != null)
                {
                    // Update the investment's current value
                    var oldInvestmentValue = investment.CurrentValue;
                    investment.CurrentValue = newMostRecent.Value;
                    
                    // Recalculate gains
                    investment.UnrealizedGainLoss = investment.CurrentValue - investment.TotalInvestment;
                    
                    // Calculate simple return
                    if (investment.TotalInvestment > 0)
                    {
                        investment.ReturnPercentage = (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100;
                    }
                    else
                    {
                        investment.ReturnPercentage = 0;
                    }

                    // Update portfolio totals if affected
                    if (investment.Portfolio != null)
                    {
                        investment.Portfolio.TotalValue = investment.Portfolio.TotalValue - oldInvestmentValue + newMostRecent.Value;
                        investment.Portfolio.UnrealizedGainLoss = investment.Portfolio.TotalValue - investment.Portfolio.TotalInvestment;
                        
                        if (investment.Portfolio.TotalInvestment > 0)
                        {
                            investment.Portfolio.ReturnPercentage = (investment.Portfolio.UnrealizedGainLoss / investment.Portfolio.TotalInvestment) * 100;
                        }
                        else
                        {
                            investment.Portfolio.ReturnPercentage = 0;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deleted history record {RecordId} for investment {InvestmentId}", 
                request.HistoryRecordId, investmentId);
                
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting history record: {Message}", ex.Message);
            return Result.Error($"Failed to delete history record: {ex.Message}");
        }
    }
} 