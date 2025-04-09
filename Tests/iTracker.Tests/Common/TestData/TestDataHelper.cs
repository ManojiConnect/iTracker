using System;
using Domain.Entities;

namespace iTracker.Tests.Common.TestData;

public static class TestDataHelper
{
    public static Investment CreateTestInvestment(
        decimal totalInvestment = 1000m,
        decimal currentValue = 1000m,
        DateTime? createdOn = null,
        string name = "Test Investment")
    {
        return new Investment
        {
            Id = 1,
            TotalInvestment = totalInvestment,
            CurrentValue = currentValue,
            CreatedOn = createdOn ?? DateTime.UtcNow,
            Name = name
        };
    }

    public static SystemSettings CreateTestSettings(
        string performanceCalculationMethod = "simple",
        int sessionTimeoutMinutes = 30)
    {
        return new SystemSettings
        {
            Id = 1,
            PerformanceCalculationMethod = performanceCalculationMethod,
            SessionTimeoutMinutes = sessionTimeoutMinutes
        };
    }

    public static InvestmentHistory CreateTestHistory(
        int investmentId = 1,
        decimal value = 1000m,
        DateTime? recordedDate = null,
        string note = "Test history record")
    {
        return new InvestmentHistory
        {
            Id = 1,
            InvestmentId = investmentId,
            Value = value,
            RecordedDate = recordedDate ?? DateTime.UtcNow,
            Note = note
        };
    }
} 