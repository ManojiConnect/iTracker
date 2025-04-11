using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Common.Responses;
using Application.Features.Investments.Common;
using Application.Features.Investments.GetAllInvestments;
using Application.Features.Portfolios.GetAllPortfolios;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using WebApp.Services;
using WebApp.Models;

namespace WebApp.Pages.Investments;

public class CategorySummary
{
    public string Name { get; set; }
    public string Color { get; set; }
    public int InvestmentCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal ReturnPercentage { get; set; }
}

public class InvestmentsListViewModel
{
    public PaginatedList<InvestmentDto> Investments { get; set; }
    public List<string> Categories { get; set; }
    public List<PortfolioDto> Portfolios { get; set; }
    public int PageSize { get; set; }
    public Func<decimal, string> FormatCurrency { get; set; }
}

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;

    [BindProperty(SupportsGet = true)]
    public string SearchText { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PortfolioId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "name";

    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "asc";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 5;

    [BindProperty(SupportsGet = true)]
    public bool ResetFilters { get; set; }

    public PaginatedList<InvestmentDto> Investments { get; private set; }
    public List<SelectListItem> CategorySelectList { get; private set; } = new();
    public List<SelectListItem> PortfolioSelectList { get; private set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<CategorySummary> CategorySummaries { get; set; } = new();
    public List<PortfolioDto> Portfolios { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; } = new();
    public InvestmentsListViewModel ListViewModel { get; set; } = new();

    public IndexModel(
        ILogger<IndexModel> logger,
        IMediator mediator,
        IApplicationSettingsService settingsService)
    {
        _logger = logger;
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Reset filters if requested
            if (ResetFilters)
            {
                SearchText = null;
                Category = null;
                PortfolioId = null;
                SortBy = "portfolio";
                SortOrder = "asc";
                PageNumber = 1;
                PageSize = 5;
            }

            // Fetch all investments first to build filters
            var allInvestmentsResult = await _mediator.Send(new GetAllInvestmentsRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue // Get all for categories, not optimal for large datasets
            });

            if (!allInvestmentsResult.IsSuccess)
            {
                _logger.LogError("Failed to fetch all investments for filter data");
                return StatusCode(500, "Failed to fetch investment data");
            }

            // Build category filter items
            var categories = allInvestmentsResult.Value.Items
                .Select(i => i.CategoryName)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CategorySelectList = categories
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = c == Category
                })
                .ToList();

            // Fetch all portfolios for filter dropdown
            var portfoliosResult = await _mediator.Send(new GetAllPortfoliosRequest());
            
            if (!portfoliosResult.IsSuccess)
            {
                _logger.LogError("Failed to fetch portfolios for filter data");
                return StatusCode(500, "Failed to fetch portfolio data");
            }

            PortfolioSelectList = portfoliosResult.Value
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = PortfolioId.HasValue && p.Id == PortfolioId.Value
                })
                .ToList();

            // Fetch paginated and filtered investments
            var request = new GetAllInvestmentsRequest
            {
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _mediator.Send(request);
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to fetch investments with filters");
                return StatusCode(500, "Failed to fetch investment data");
            }

            // Apply manual filtering since the handler might not support all filters
            var filteredInvestments = result.Value.Items.AsEnumerable();
            
            // Apply portfolio filter if specified
            if (PortfolioId.HasValue)
            {
                filteredInvestments = filteredInvestments.Where(i => i.PortfolioId == PortfolioId.Value);
            }
            
            // Apply search filter if specified
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredInvestments = filteredInvestments.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.PortfolioName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply category filter if specified
            if (!string.IsNullOrWhiteSpace(Category))
            {
                filteredInvestments = filteredInvestments.Where(i => i.CategoryName == Category);
            }
            
            // Apply sorting
            filteredInvestments = SortBy.ToLower() switch
            {
                "name" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.Name) : filteredInvestments.OrderByDescending(i => i.Name),
                "category" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.CategoryName) : filteredInvestments.OrderByDescending(i => i.CategoryName),
                "portfolio" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.PortfolioName) : filteredInvestments.OrderByDescending(i => i.PortfolioName),
                "initialinvestment" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.TotalInvestment) : filteredInvestments.OrderByDescending(i => i.TotalInvestment),
                "currentvalue" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.CurrentValue) : filteredInvestments.OrderByDescending(i => i.CurrentValue),
                "return" => SortOrder == "asc" ? filteredInvestments.OrderBy(i => i.ReturnPercentage) : filteredInvestments.OrderByDescending(i => i.ReturnPercentage),
                _ => filteredInvestments.OrderBy(i => i.Name)
            };
            
            // Manually implement pagination
            var totalCount = filteredInvestments.Count();
            var pagedItems = filteredInvestments
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            Investments = new PaginatedList<InvestmentDto>(pagedItems, totalCount, PageNumber, PageSize);

            // Get all investments first to get the complete list of categories
            var allInvestmentsResultAgain = await _mediator.Send(new GetAllInvestmentsRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue // Get all investments to build complete category list
            });
            
            if (!allInvestmentsResultAgain.IsSuccess)
            {
                return StatusCode(500, "Failed to fetch investments");
            }

            // Get all categories for the filter dropdown from all investments
            Categories = allInvestmentsResultAgain.Value.Items
                .Select(i => i.CategoryName)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Calculate category summaries
            CategorySummaries = Investments.Items
                .GroupBy(i => i.CategoryName)
                .Select(g => new CategorySummary
                {
                    Name = g.Key,
                    Color = GetCategoryColor(g.Key),
                    InvestmentCount = g.Count(),
                    TotalValue = g.Sum(i => i.CurrentValue),
                    UnrealizedGainLoss = g.Sum(i => i.UnrealizedGainLoss),
                    ReturnPercentage = g.Sum(i => i.TotalInvestment) > 0
                        ? (g.Sum(i => i.UnrealizedGainLoss) / g.Sum(i => i.TotalInvestment)) * 100
                        : 0
                })
                .OrderBy(s => s.Name)
                .ToList();

            // Get all portfolios for the filter dropdown
            var portfoliosResultAgain = await _mediator.Send(new GetAllPortfoliosRequest());
            if (!portfoliosResultAgain.IsSuccess)
            {
                return StatusCode(500, "Failed to fetch portfolios");
            }
            Portfolios = portfoliosResultAgain.Value.ToList();

            // Get system settings
            Settings = await _settingsService.GetSettingsAsync();

            // Initialize the list view model
            ListViewModel = new InvestmentsListViewModel
            {
                Investments = Investments,
                Categories = Categories,
                Portfolios = Portfolios,
                PageSize = PageSize,
                FormatCurrency = _settingsService.FormatCurrency
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching investments");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    private string GetCategoryColor(string category)
    {
        return category.ToLower() switch
        {
            "stocks" => "#4CAF50",
            "bonds" => "#2196F3",
            "real estate" => "#FF9800",
            "cryptocurrency" => "#9C27B0",
            "commodities" => "#F44336",
            "cash" => "#607D8B",
            _ => "#757575"
        };
    }

    public string FormatCurrency(decimal amount)
    {
        return _settingsService.FormatCurrency(amount);
    }
    
    public string FormatNumber(decimal number, int? decimalPlaces = null)
    {
        return _settingsService.FormatNumber(number, decimalPlaces);
    }
} 