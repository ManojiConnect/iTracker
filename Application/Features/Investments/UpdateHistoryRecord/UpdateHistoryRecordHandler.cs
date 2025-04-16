using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Investments.UpdateHistoryRecord;

public class UpdateHistoryRecordHandler : IRequestHandler<UpdateHistoryRecordRequest, Result>
{
    private readonly IContext _context;
    private readonly ILogger<UpdateHistoryRecordHandler> _logger;

    public UpdateHistoryRecordHandler(IContext context, ILogger<UpdateHistoryRecordHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateHistoryRecordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the input value
            if (request.Value < 0 || request.Value > 999999999999m)
            {
                _logger.LogWarning("Invalid value {Value} for history record update", request.Value);
                return Result.Error("Value must be between 0 and 999,999,999,999");
            }

            // Find the history record
            var historyRecord = await _context.InvestmentHistories
                .Include(h => h.Investment)
                .ThenInclude(i => i.Portfolio)
                .FirstOrDefaultAsync(h => h.Id == request.HistoryRecordId, cancellationToken);

            if (historyRecord == null)
            {
                _logger.LogWarning("History record with ID {RecordId} not found", request.HistoryRecordId);
                return Result.NotFound("History record not found");
            }

            // Store the old value for calculations
            var oldValue = historyRecord.Value;
            
            // Update the history record
            historyRecord.Value = request.Value;
            historyRecord.Note = request.Note;

            // Check if this is the most recent record
            var isMostRecent = await _context.InvestmentHistories
                .Where(h => h.InvestmentId == historyRecord.InvestmentId)
                .OrderByDescending(h => h.RecordedDate)
                .Select(h => h.Id)
                .FirstOrDefaultAsync(cancellationToken) == historyRecord.Id;

            // If this is the most recent record, update the investment's current value
            if (isMostRecent && historyRecord.Investment != null)
            {
                var investment = historyRecord.Investment;
                var oldInvestmentValue = investment.CurrentValue;
                
                // Update the investment's current value
                investment.CurrentValue = request.Value;
                
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
                    investment.Portfolio.TotalValue = investment.Portfolio.TotalValue - oldInvestmentValue + request.Value;
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

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated history record {RecordId} for investment {InvestmentId}", 
                historyRecord.Id, historyRecord.InvestmentId);
                
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating history record: {Message}", ex.Message);
            return Result.Error($"Failed to update history record: {ex.Message}");
        }
    }
} 