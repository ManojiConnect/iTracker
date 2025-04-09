using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class SystemSettingsViewModel
{
    // Currency Settings
    [Display(Name = "Currency Symbol")]
    public string CurrencySymbol { get; set; } = "$";
    
    [Display(Name = "Decimal Separator")]
    public string DecimalSeparator { get; set; } = ".";
    
    [Display(Name = "Thousands Separator")]
    public string ThousandsSeparator { get; set; } = ",";
    
    [Display(Name = "Decimal Places")]
    [Range(0, 4, ErrorMessage = "Decimal places must be between 0 and 4")]
    public int DecimalPlaces { get; set; } = 2;

    // Date & Time Settings
    [Display(Name = "Date Format")]
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    
    [Display(Name = "Financial Year Start Month")]
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
    public int FinancialYearStartMonth { get; set; } = 4;

    // Investment Settings
    [Display(Name = "Performance Calculation Method")]
    public string PerformanceCalculationMethod { get; set; } = "simple";

    // System Settings
    [Display(Name = "Session Timeout (minutes)")]
    [Range(5, 120, ErrorMessage = "Session timeout must be between 5 and 120 minutes")]
    public int SessionTimeoutMinutes { get; set; } = 30;

    // View Settings
    [Display(Name = "Default Portfolio View")]
    public string DefaultPortfolioView { get; set; } = "list";
} 