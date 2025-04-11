using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WebApp.Models;
using Application.Abstractions.Services;
using Domain.Entities;

namespace WebApp.Services;

public interface IApplicationSettingsService
{
    Task<SystemSettingsViewModel> GetSettingsAsync();
    Task<bool> SaveSettingsAsync(SystemSettingsViewModel settings);
    string FormatCurrency(decimal amount);
    string FormatNumber(decimal number, int? decimalPlaces = null);
    string FormatDate(DateTime date);
    Task InitializeSettingsAsync();
}

public class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly string _settingsFilePath;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApplicationSettingsService> _logger;
    private readonly IMemoryCache _cache;
    private readonly ISettingsService _dbSettingsService;
    private const string SettingsCacheKey = "AppSettings";
    
    public ApplicationSettingsService(
        IWebHostEnvironment environment,
        ILogger<ApplicationSettingsService> logger,
        IMemoryCache cache,
        ISettingsService dbSettingsService)
    {
        _environment = environment;
        _logger = logger;
        _cache = cache;
        _dbSettingsService = dbSettingsService;
        
        // Store settings in a JSON file in the App_Data directory
        var appDataDir = Path.Combine(_environment.ContentRootPath, "App_Data");
        _settingsFilePath = Path.Combine(appDataDir, "settings.json");
    }
    
    /// <summary>
    /// Initialize settings on application startup. This will load settings from file
    /// and ensure they are synced to the database, but only if the database has default values.
    /// </summary>
    public async Task InitializeSettingsAsync()
    {
        try
        {
            _logger.LogInformation("Initializing application settings");
            
            // Get settings from database first
            var dbSettings = await _dbSettingsService.GetAllSettingsAsync();
            
            // Check if db settings has a valid currency symbol
            bool hasValidCurrencySymbol = !string.IsNullOrEmpty(dbSettings.CurrencySymbol) && dbSettings.CurrencySymbol != "?";
            
            // If the database has the default dollar sign or an invalid symbol, try to load from file and update DB
            if ((dbSettings.CurrencySymbol == "$" || !hasValidCurrencySymbol) && File.Exists(_settingsFilePath))
            {
                _logger.LogInformation("Database has default settings, syncing from file");
                
                // Load settings from file
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var fileSettings = JsonSerializer.Deserialize<SystemSettingsViewModel>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (fileSettings != null)
                {
                    // Map file settings to domain model
                    dbSettings.CurrencySymbol = fileSettings.CurrencySymbol;
                    dbSettings.DecimalSeparator = fileSettings.DecimalSeparator;
                    dbSettings.ThousandsSeparator = fileSettings.ThousandsSeparator;
                    dbSettings.DecimalPlaces = fileSettings.DecimalPlaces;
                    dbSettings.DateFormat = fileSettings.DateFormat;
                    dbSettings.FinancialYearStartMonth = fileSettings.FinancialYearStartMonth;
                    dbSettings.PerformanceCalculationMethod = fileSettings.PerformanceCalculationMethod;
                    dbSettings.SessionTimeoutMinutes = fileSettings.SessionTimeoutMinutes;
                    dbSettings.DefaultPortfolioView = fileSettings.DefaultPortfolioView;
                    
                    // Update database with file settings
                    var success = await _dbSettingsService.UpdateSettingsAsync(dbSettings);
                    
                    if (success)
                    {
                        _logger.LogInformation("Successfully synchronized settings from file to database");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update database with file settings");
                    }
                }
            }
            else if (!hasValidCurrencySymbol)
            {
                // Set a default currency symbol if one isn't valid
                _logger.LogInformation("Setting default currency symbol (₹)");
                dbSettings.CurrencySymbol = "₹";
                await _dbSettingsService.UpdateSettingsAsync(dbSettings);
            }
            else
            {
                _logger.LogInformation("Using existing database settings (CurrencySymbol={Symbol})", 
                    dbSettings.CurrencySymbol);
            }
            
            // Clear any existing cache to ensure we're using the latest settings
            _cache.Remove(SettingsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing application settings");
        }
    }
    
    public async Task<SystemSettingsViewModel> GetSettingsAsync()
    {
        try
        {
            // First try to get from cache
            if (_cache.TryGetValue(SettingsCacheKey, out SystemSettingsViewModel cachedSettings) && 
                cachedSettings != null)
            {
                return cachedSettings;
            }
            
            // Get settings from database (source of truth)
            var dbSettings = await _dbSettingsService.GetAllSettingsAsync();
            
            // Map domain model to view model
            var viewModel = new SystemSettingsViewModel
            {
                CurrencySymbol = dbSettings.CurrencySymbol,
                DecimalSeparator = dbSettings.DecimalSeparator,
                ThousandsSeparator = dbSettings.ThousandsSeparator,
                DecimalPlaces = dbSettings.DecimalPlaces,
                DateFormat = dbSettings.DateFormat,
                FinancialYearStartMonth = dbSettings.FinancialYearStartMonth,
                PerformanceCalculationMethod = dbSettings.PerformanceCalculationMethod,
                SessionTimeoutMinutes = dbSettings.SessionTimeoutMinutes,
                DefaultPortfolioView = dbSettings.DefaultPortfolioView
            };
            
            // Cache the settings
            _cache.Set(SettingsCacheKey, viewModel, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Settings loaded from database: CurrencySymbol={Symbol}", viewModel.CurrencySymbol);
            
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings from database");
            return GetDefaultSettings();
        }
    }
    
    public async Task<bool> SaveSettingsAsync(SystemSettingsViewModel settings)
    {
        try
        {
            // Get current database settings to update
            var dbSettings = await _dbSettingsService.GetAllSettingsAsync();
            
            // Map view model to domain model
            dbSettings.CurrencySymbol = settings.CurrencySymbol;
            dbSettings.DecimalSeparator = settings.DecimalSeparator;
            dbSettings.ThousandsSeparator = settings.ThousandsSeparator;
            dbSettings.DecimalPlaces = settings.DecimalPlaces;
            dbSettings.DateFormat = settings.DateFormat;
            dbSettings.FinancialYearStartMonth = settings.FinancialYearStartMonth;
            dbSettings.PerformanceCalculationMethod = settings.PerformanceCalculationMethod;
            dbSettings.SessionTimeoutMinutes = settings.SessionTimeoutMinutes;
            dbSettings.DefaultPortfolioView = settings.DefaultPortfolioView;
            
            // Save to database (primary source of truth)
            var success = await _dbSettingsService.UpdateSettingsAsync(dbSettings);
            
            if (success)
            {
                // Also save to file as backup
                await SaveToFileAsync(settings);
                
                // Clear cache to ensure fresh data
                _cache.Remove(SettingsCacheKey);
                
                _logger.LogInformation("Settings saved to database and file: CurrencySymbol={Symbol}", 
                    settings.CurrencySymbol);
            }
            else
            {
                _logger.LogWarning("Failed to save settings to database");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            return false;
        }
    }
    
    private async Task<bool> SaveToFileAsync(SystemSettingsViewModel settings)
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
        try
        {
            // Get settings (cached if available)
            var settings = GetSettingsAsync().GetAwaiter().GetResult();
            
            // Ensure currency symbol is valid
            if (string.IsNullOrEmpty(settings.CurrencySymbol) || settings.CurrencySymbol == "?")
            {
                _logger.LogWarning("Invalid currency symbol detected, using default ₹");
                settings.CurrencySymbol = "₹";
                // Update cache with valid symbol
                _cache.Set(SettingsCacheKey, settings, TimeSpan.FromMinutes(5));
            }
            
            string formattedNumber = FormatNumber(amount, settings.DecimalPlaces);
            return $"{settings.CurrencySymbol}{formattedNumber}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting currency, using fallback");
            return $"₹{amount:N2}";
        }
    }
    
    public string FormatNumber(decimal number, int? decimalPlaces = null)
    {
        // Get settings (cached if available)
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
        _logger.LogInformation("Creating default application settings with ₹ symbol");
        
        var defaultSettings = new SystemSettingsViewModel
        {
            CurrencySymbol = "₹", // Default currency symbol is Rupee
            DecimalSeparator = ".",
            ThousandsSeparator = ",",
            DecimalPlaces = 2,
            DateFormat = "dd/MM/yyyy",
            FinancialYearStartMonth = 4,
            PerformanceCalculationMethod = "simple",
            SessionTimeoutMinutes = 30,
            DefaultPortfolioView = "list"
        };
        
        // Cache the default settings
        _cache.Set(SettingsCacheKey, defaultSettings, TimeSpan.FromMinutes(5));
        
        return defaultSettings;
    }
} 