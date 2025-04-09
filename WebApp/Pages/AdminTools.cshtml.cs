using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using WebApp.Models;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using WebApp.Services;

namespace WebApp.Pages;

public class AdminToolsModel : PageModel
{
    private readonly IApplicationSettingsService _settingsService;
    private readonly ILogger<AdminToolsModel> _logger;

    [TempData]
    public string SuccessMessage { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }
    
    [BindProperty]
    public SystemSettingsViewModel Settings { get; set; } = new SystemSettingsViewModel();

    public AdminToolsModel(IApplicationSettingsService settingsService, ILogger<AdminToolsModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            // Get settings from service
            Settings = await _settingsService.GetSettingsAsync();
            _logger.LogInformation("Settings loaded successfully: CurrencySymbol={Symbol}", Settings.CurrencySymbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
            ErrorMessage = $"Error loading settings: {ex.Message}";
            
            // Use default values if loading fails
            Settings = new SystemSettingsViewModel
            {
                CurrencySymbol = "$",
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                DecimalPlaces = 2,
                DateFormat = "MM/dd/yyyy",
                FinancialYearStartMonth = 4,
                PerformanceCalculationMethod = "simple",
                SessionTimeoutMinutes = 30
            };
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Validate settings
            if (Settings.DecimalSeparator == Settings.ThousandsSeparator)
            {
                ModelState.AddModelError(string.Empty, "Decimal separator and thousands separator cannot be the same.");
                return Page();
            }

            _logger.LogInformation("Saving settings: CurrencySymbol={Symbol}", Settings.CurrencySymbol);
            
            // Save settings
            var success = await _settingsService.SaveSettingsAsync(Settings);
            
            if (success)
            {
                // Force refresh of settings in memory
                Settings = await _settingsService.GetSettingsAsync();
                _logger.LogInformation("Settings saved and refreshed: {Symbol}", Settings.CurrencySymbol);
                SuccessMessage = "Settings updated successfully.";
            }
            else
            {
                ErrorMessage = "Failed to update settings.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return RedirectToPage();
    }
} 