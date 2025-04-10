using System;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SettingsService> _logger;
    private const string SettingsCacheKey = "SystemSettings";

    public SettingsService(IContext context, IMemoryCache cache, ILogger<SettingsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SystemSettings> GetAllSettingsAsync()
    {
        // Try to get from cache
        if (_cache.TryGetValue(SettingsCacheKey, out SystemSettings? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        // Get from database or initialize new settings
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            // Initialize default settings
            settings = new SystemSettings
            {
                CurrencySymbol = "$",
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                DecimalPlaces = 2,
                DateFormat = "MM/dd/yyyy",
                FinancialYearStartMonth = 4,
                PerformanceCalculationMethod = "simple",
                SessionTimeoutMinutes = 30,
                DefaultPortfolioView = "list",
                MinPasswordLength = 8
            };
            
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        // Cache the settings
        _cache.Set(SettingsCacheKey, settings, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("Retrieved settings with currency symbol: {Symbol}", settings.CurrencySymbol);
        return settings;
    }

    public async Task<string> GetSettingAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        var settings = await GetAllSettingsAsync();
        
        // Use reflection to get property value
        var property = settings.GetType().GetProperty(key);
        if (property != null)
        {
            var value = property.GetValue(settings);
            return value?.ToString() ?? string.Empty;
        }
        
        // Try to get from key-value settings
        var keyValueSetting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        
        return keyValueSetting?.SettingValue ?? string.Empty;
    }

    public async Task<bool> UpdateSettingsAsync(SystemSettings updatedSettings)
    {
        if (updatedSettings == null)
        {
            return false;
        }

        try
        {
            var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (existingSettings == null)
            {
                _context.SystemSettings.Add(updatedSettings);
            }
            else
            {
                // Update all properties
                existingSettings.CurrencySymbol = updatedSettings.CurrencySymbol;
                existingSettings.DecimalSeparator = updatedSettings.DecimalSeparator;
                existingSettings.ThousandsSeparator = updatedSettings.ThousandsSeparator;
                existingSettings.DecimalPlaces = updatedSettings.DecimalPlaces;
                existingSettings.DateFormat = updatedSettings.DateFormat;
                existingSettings.FinancialYearStartMonth = updatedSettings.FinancialYearStartMonth;
                existingSettings.PerformanceCalculationMethod = updatedSettings.PerformanceCalculationMethod;
                existingSettings.SessionTimeoutMinutes = updatedSettings.SessionTimeoutMinutes;
                existingSettings.DefaultPortfolioView = updatedSettings.DefaultPortfolioView;
                existingSettings.MinPasswordLength = updatedSettings.MinPasswordLength;
                
                _context.SystemSettings.Update(existingSettings);
            }
            
            await _context.SaveChangesAsync();
            
            // Invalidate cache
            InvalidateCache();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return false;
        }
    }

    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        try
        {
            var settings = await GetAllSettingsAsync();
            
            // Try to update property using reflection
            var property = settings.GetType().GetProperty(key);
            if (property != null)
            {
                // Convert value to property type
                var typedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(settings, typedValue);
                
                await UpdateSettingsAsync(settings);
                return true;
            }
            
            // If property doesn't exist, return false
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating setting {Key} to {Value}", key, value);
            return false;
        }
    }
    
    public void InvalidateCache()
    {
        _cache.Remove(SettingsCacheKey);
        _logger.LogInformation("Settings cache manually invalidated");
    }
} 