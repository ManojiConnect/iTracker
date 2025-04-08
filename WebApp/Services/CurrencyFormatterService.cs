using System;
using System.Globalization;
using System.Threading.Tasks;
using Application.Abstractions.Services;
using Domain.Entities;

namespace WebApp.Services;

public class CurrencyFormatterService
{
    private readonly ISettingsService _settingsService;
    private SystemSettings _cachedSettings;
    private DateTime _cacheExpiry = DateTime.MinValue;

    public CurrencyFormatterService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
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
        return settings.CurrencySymbol;
    }

    private async Task<SystemSettings> GetSettingsAsync()
    {
        // Simple caching mechanism to avoid unnecessary database calls
        if (_cachedSettings == null || DateTime.UtcNow > _cacheExpiry)
        {
            _cachedSettings = await _settingsService.GetAllSettingsAsync();
            _cacheExpiry = DateTime.UtcNow.AddMinutes(5); // Cache for 5 minutes
        }
        
        return _cachedSettings;
    }
} 