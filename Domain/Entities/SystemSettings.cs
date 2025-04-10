using Domain.Entities.Common;

namespace Domain.Entities;

public class SystemSettings : Entity
{
    // Currency Settings
    public string CurrencySymbol { get; set; } = "$";
    public string DecimalSeparator { get; set; } = ".";
    public string ThousandsSeparator { get; set; } = ",";
    public int DecimalPlaces { get; set; } = 2;

    // Date & Time Settings
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public int FinancialYearStartMonth { get; set; } = 4; // April

    // Investment Settings
    public string PerformanceCalculationMethod { get; set; } = "simple";

    // View Settings
    public string DefaultPortfolioView { get; set; } = "list";

    // System Settings
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MinPasswordLength { get; set; } = 8;
    
    // Additional Settings
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    
    // Constructor
    public SystemSettings()
    {
    }
} 