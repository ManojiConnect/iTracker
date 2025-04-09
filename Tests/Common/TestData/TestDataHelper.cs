using System;
using Domain.Entities;

namespace iTracker.Tests.Common.TestData;

public static class TestDataHelper
{
    public static Investment CreateTestInvestment(
        decimal totalInvestment = 1000m,
        decimal currentValue = 1200m,
        DateTime? createdOn = null,
        string name = "Test Investment")
    {
        return new Investment
        {
            Name = name,
            TotalInvestment = totalInvestment,
            CurrentValue = currentValue,
            CreatedOn = createdOn ?? DateTime.UtcNow.AddYears(-1)
        };
    }

    public static SystemSettings CreateTestSettings(
        string performanceCalculationMethod = "simple",
        int sessionTimeoutMinutes = 30)
    {
        return new SystemSettings
        {
            PerformanceCalculationMethod = performanceCalculationMethod,
            SessionTimeoutMinutes = sessionTimeoutMinutes,
            CurrencySymbol = "$",
            DecimalSeparator = ".",
            ThousandsSeparator = ",",
            DecimalPlaces = 2,
            DateFormat = "MM/dd/yyyy",
            FinancialYearStartMonth = 4
        };
    }

    public static InvestmentHistory CreateTestHistory(
        int investmentId,
        decimal value,
        DateTime? recordedDate = null,
        string note = "Test update")
    {
        return new InvestmentHistory
        {
            InvestmentId = investmentId,
            Value = value,
            RecordedDate = recordedDate ?? DateTime.UtcNow,
            Note = note
        };
    }
} 