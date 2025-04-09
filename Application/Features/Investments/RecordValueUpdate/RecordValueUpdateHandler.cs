using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.RecordValueUpdate;

public class RecordValueUpdateHandler : IRequestHandler<RecordValueUpdateRequest, Result<int>>
{
    private readonly IContext _context;
    private readonly ISettingsService _settingsService;

    public RecordValueUpdateHandler(IContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
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
        
        // Recalculate gains
        investment.UnrealizedGainLoss = investment.CurrentValue - investment.TotalInvestment;

        // Get performance calculation method from settings
        var settings = await _settingsService.GetAllSettingsAsync();
        var calculationMethod = settings.PerformanceCalculationMethod?.ToLower() ?? "simple";

        // Calculate return percentage based on the selected method
        if (investment.TotalInvestment > 0)
        {
            if (calculationMethod == "annualized")
            {
                // Get the investment's start date (first history record or creation date)
                var startDate = await _context.InvestmentHistories
                    .Where(h => h.InvestmentId == investment.Id)
                    .OrderBy(h => h.RecordedDate)
                    .Select(h => h.RecordedDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (startDate == default)
                {
                    startDate = investment.CreatedOn;
                }

                var timeSpan = DateTime.UtcNow - startDate;
                var years = timeSpan.TotalDays / 365.25; // Using 365.25 to account for leap years

                if (years > 0)
                {
                    // Annualized Return = (1 + Total Return)^(1/Years) - 1
                    var totalReturn = investment.UnrealizedGainLoss / investment.TotalInvestment;
                    investment.ReturnPercentage = (decimal)(Math.Pow((double)(1 + totalReturn), 1.0 / years) - 1) * 100;
                }
                else
                {
                    // If less than a year, use simple return
                    investment.ReturnPercentage = (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100;
                }
            }
            else
            {
                // Simple return calculation
                investment.ReturnPercentage = (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100;
            }
        }
        else
        {
            investment.ReturnPercentage = 0;
        }

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