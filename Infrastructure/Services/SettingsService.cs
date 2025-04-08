using System;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IContext _context;
    private readonly IMemoryCache _cache;
    private const string SettingsCacheKey = "SystemSettings";

    public SettingsService(IContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SystemSettings> GetAllSettingsAsync()
    {
        // Try to get from cache
        if (_cache.TryGetValue(SettingsCacheKey, out SystemSettings cachedSettings))
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
                DefaultPortfolioView = "list",
                PerformanceCalculationMethod = "simple",
                SessionTimeoutMinutes = 30,
                MinPasswordLength = 8
            };
            
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        // Cache the settings
        _cache.Set(SettingsCacheKey, settings, TimeSpan.FromMinutes(30));
        
        return settings;
    }

    public async Task<string> GetSettingAsync(string key)
    {
        var settings = await GetAllSettingsAsync();
        
        // Use reflection to get property value
        var property = settings.GetType().GetProperty(key);
        if (property != null)
        {
            return property.GetValue(settings)?.ToString() ?? string.Empty;
        }
        
        // Try to get from key-value settings
        var keyValueSetting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        
        return keyValueSetting?.SettingValue ?? string.Empty;
    }

    public async Task<bool> UpdateSettingsAsync(SystemSettings updatedSettings)
    {
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
                existingSettings.DefaultPortfolioView = updatedSettings.DefaultPortfolioView;
                existingSettings.PerformanceCalculationMethod = updatedSettings.PerformanceCalculationMethod;
                existingSettings.SessionTimeoutMinutes = updatedSettings.SessionTimeoutMinutes;
                existingSettings.MinPasswordLength = updatedSettings.MinPasswordLength;
                
                _context.SystemSettings.Update(existingSettings);
            }
            
            await _context.SaveChangesAsync();
            
            // Invalidate cache
            _cache.Remove(SettingsCacheKey);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
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
            
            // Try to update key-value setting
            var keyValueSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key);
            
            if (keyValueSetting == null)
            {
                keyValueSetting = new SystemSettings
                {
                    SettingKey = key,
                    SettingValue = value
                };
                _context.SystemSettings.Add(keyValueSetting);
            }
            else
            {
                keyValueSetting.SettingValue = value;
                _context.SystemSettings.Update(keyValueSetting);
            }
            
            await _context.SaveChangesAsync();
            
            // Invalidate cache
            _cache.Remove(SettingsCacheKey);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
} 