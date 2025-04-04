using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using WebApp.Models;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebApp.Controllers;

[Route("[controller]")]
public class InvestmentsController : Controller
{
    private readonly IApplicationSettingsService _settingsService;

    public InvestmentsController(IApplicationSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpPost]
    [Route("SaveViewPreference")]
    public async Task<IActionResult> SaveViewPreference(string viewType)
    {
        if (string.IsNullOrEmpty(viewType) || (viewType != "list" && viewType != "grid"))
        {
            return BadRequest("Invalid view type");
        }

        var settings = await _settingsService.GetSettingsAsync();
        settings.DefaultPortfolioView = viewType;
        
        // Use the service to save the settings
        var success = await _settingsService.SaveSettingsAsync(settings);
        
        if (!success)
        {
            return StatusCode(500, "Failed to save settings");
        }
        
        return Ok();
    }
} 