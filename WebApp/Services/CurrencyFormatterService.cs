using System;
using System.Globalization;
using System.Threading.Tasks;
using Application.Abstractions.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace WebApp.Services;

public class CurrencyFormatterService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<CurrencyFormatterService> _logger;

    public CurrencyFormatterService(ISettingsService settingsService, ILogger<CurrencyFormatterService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<string> FormatCurrencyAsync(decimal amount)
    {
        var settings = await GetSettingsAsync();
        
        var numberFormat = new NumberFormatInfo
        {
            CurrencySymbol = settings.CurrencySymbol,
            CurrencyDecimalSeparator = settings.DecimalSeparator,
            CurrencyGroupSeparator = settings.ThousandsSeparator,
            CurrencyDecimalDigits = settings.DecimalPlaces
        };
        
        return string.Format(numberFormat, "{0:C}", amount);
    }
    
    public async Task<string> GetCurrencySymbolAsync()
    {
        var settings = await GetSettingsAsync();
        _logger.LogInformation("Retrieved currency symbol from settings: {Symbol}", settings.CurrencySymbol);
        return settings.CurrencySymbol;
    }
    
    private async Task<SystemSettings> GetSettingsAsync()
    {
        // Force invalidate cache to always get the most current settings
        _settingsService.InvalidateCache();
        return await _settingsService.GetAllSettingsAsync();
    }
} 