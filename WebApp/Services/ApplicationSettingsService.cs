using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WebApp.Models;

namespace WebApp.Services;

public interface IApplicationSettingsService
{
    Task<SystemSettingsViewModel> GetSettingsAsync();
    Task<bool> SaveSettingsAsync(SystemSettingsViewModel settings);
    string FormatCurrency(decimal amount);
    string FormatNumber(decimal number, int? decimalPlaces = null);
    string FormatDate(DateTime date);
}

public class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly string _settingsFilePath;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApplicationSettingsService> _logger;
    private readonly IMemoryCache _cache;
    private const string SettingsCacheKey = "AppSettings";
    
    public ApplicationSettingsService(IWebHostEnvironment environment, 
                                      ILogger<ApplicationSettingsService> logger,
                                      IMemoryCache cache)
    {
        _environment = environment;
        _logger = logger;
        _cache = cache;
        
        // Store settings in a JSON file in the App_Data directory
        var appDataDir = Path.Combine(_environment.ContentRootPath, "App_Data");
        _settingsFilePath = Path.Combine(appDataDir, "settings.json");
    }
    
    public async Task<SystemSettingsViewModel> GetSettingsAsync()
    {
        // Try to get from cache first
        if (_cache.TryGetValue(SettingsCacheKey, out SystemSettingsViewModel cachedSettings))
        {
            return cachedSettings;
        }
        
        try
        {
            // Try to load settings from the JSON file
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var settings = JsonSerializer.Deserialize<SystemSettingsViewModel>(json, options);
                
                if (settings != null)
                {
                    // Cache the settings for 5 minutes
                    _cache.Set(SettingsCacheKey, settings, TimeSpan.FromMinutes(5));
                    return settings;
                }
            }
            
            // Return default settings if file doesn't exist or deserialization failed
            return GetDefaultSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings from file");
            return GetDefaultSettings();
        }
    }
    
    public async Task<bool> SaveSettingsAsync(SystemSettingsViewModel settings)
    {
        try
        {
            // Ensure the directory exists
            var appDataDir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }
            
            // Serialize settings to JSON and save to file
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            
            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            
            // Update the cache
            _cache.Set(SettingsCacheKey, settings, TimeSpan.FromMinutes(5));
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to file");
            return false;
        }
    }
    
    public string FormatCurrency(decimal amount)
    {
        var settings = GetSettingsAsync().GetAwaiter().GetResult();
        
        string formattedNumber = FormatNumber(amount, settings.DecimalPlaces);
        return $"{settings.CurrencySymbol}{formattedNumber}";
    }
    
    public string FormatNumber(decimal number, int? decimalPlaces = null)
    {
        var settings = GetSettingsAsync().GetAwaiter().GetResult();
        
        // Use the specified decimal places or the default from settings
        int places = decimalPlaces ?? settings.DecimalPlaces;
        
        // Round to the specified number of decimal places
        decimal roundedNumber = Math.Round(number, places);
        
        // Convert to string with the correct number of decimal places
        string numberStr = roundedNumber.ToString($"F{places}");
        
        // Replace the decimal separator
        numberStr = numberStr.Replace(".", settings.DecimalSeparator);
        
        // Add thousands separators
        int decimalPos = numberStr.IndexOf(settings.DecimalSeparator);
        if (decimalPos < 0) decimalPos = numberStr.Length;
        
        for (int i = decimalPos - 3; i > 0; i -= 3)
        {
            numberStr = numberStr.Insert(i, settings.ThousandsSeparator);
        }
        
        return numberStr;
    }
    
    public string FormatDate(DateTime date)
    {
        var settings = GetSettingsAsync().GetAwaiter().GetResult();
        return date.ToString(settings.DateFormat);
    }
    
    private SystemSettingsViewModel GetDefaultSettings()
    {
        var defaultSettings = new SystemSettingsViewModel
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
        
        // Cache the default settings
        _cache.Set(SettingsCacheKey, defaultSettings, TimeSpan.FromMinutes(5));
        
        return defaultSettings;
    }
} 