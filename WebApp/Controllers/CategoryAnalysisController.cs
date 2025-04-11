using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CategoryAnalysisController : Controller
{
    [HttpGet("/CategoryAnalysis")]
    public IActionResult Index(int? portfolioId)
    {
        return RedirectToPage("/Investments/CategoryAnalysis", new { portfolioId });
    }
} 