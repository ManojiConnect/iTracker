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
        try
        {
            // Validate the input value
            if (request.NewValue < 0 || request.NewValue > 999999999999m)
            {
                return Result.Error("Value must be between 0 and 999,999,999,999");
            }
            
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

            // Calculate simple return percentage (most reliable method)
            decimal simpleReturn = 0;
            if (investment.TotalInvestment > 0)
            {
                simpleReturn = (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100;
            }

            // Set the return percentage to simple return by default
            investment.ReturnPercentage = simpleReturn;

            // Get performance calculation method from settings
            var settings = await _settingsService.GetAllSettingsAsync();
            var calculationMethod = settings.PerformanceCalculationMethod?.ToLower() ?? "simple";

            // Only attempt annualized calculation if method is set to "annualized" and there's a positive investment amount
            if (calculationMethod == "annualized" && investment.TotalInvestment > 0)
            {
                try
                {
                    // Get the investment's start date (purchase date or first history record)
                    var startDate = investment.PurchaseDate;
                    if (startDate == default)
                    {
                        var firstHistoryDate = await _context.InvestmentHistories
                            .Where(h => h.InvestmentId == investment.Id)
                            .OrderBy(h => h.RecordedDate)
                            .Select(h => h.RecordedDate)
                            .FirstOrDefaultAsync(cancellationToken);
                            
                        // If no history exists, use yesterday as fallback
                        startDate = firstHistoryDate != default ? firstHistoryDate : DateTime.UtcNow.AddDays(-1);
                    }

                    var timeSpan = DateTime.UtcNow - startDate;
                    var years = timeSpan.TotalDays / 365.25; // Using 365.25 to account for leap years

                    // Only calculate annualized return if we have a meaningful time period (at least 30 days)
                    if (years >= 0.08)
                    {
                        // Total return as decimal (e.g., 25% = 0.25)
                        var totalReturnDecimal = (double)(investment.UnrealizedGainLoss / investment.TotalInvestment);
                        
                        // Avoid negative base in power calculation
                        if (1 + totalReturnDecimal > 0)
                        {
                            // Annualized Return = (1 + Total Return)^(1/Years) - 1
                            var annualizedReturn = Math.Pow(1 + totalReturnDecimal, 1.0 / years) - 1;
                            
                            // Check if the result is valid and within reasonable bounds
                            if (!double.IsInfinity(annualizedReturn) && !double.IsNaN(annualizedReturn) &&
                                annualizedReturn >= -1 && annualizedReturn <= 10)
                            {
                                // Convert to percentage (multiply by 100)
                                investment.ReturnPercentage = (decimal)annualizedReturn * 100;
                            }
                        }
                    }
                }
                catch
                {
                    // If any calculation fails, we already have the simple return set
                }
            }

            // Update the portfolio totals
            if (investment.Portfolio != null)
            {
                investment.Portfolio.TotalValue = investment.Portfolio.TotalValue - oldValue + request.NewValue;
                investment.Portfolio.UnrealizedGainLoss = investment.Portfolio.TotalValue - investment.Portfolio.TotalInvestment;
                
                // Always use simple return for portfolio
                investment.Portfolio.ReturnPercentage = investment.Portfolio.TotalInvestment > 0
                    ? (investment.Portfolio.UnrealizedGainLoss / investment.Portfolio.TotalInvestment) * 100
                    : 0;
            }

            // Save the changes
            _context.InvestmentHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(history.Id);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
} 