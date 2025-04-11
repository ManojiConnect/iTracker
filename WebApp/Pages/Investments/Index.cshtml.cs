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
using Application.Features.Portfolios.GetPortfolioById;
using PortfolioListDto = Application.Features.Portfolios.GetAllPortfolios.PortfolioDto;
using PortfolioDetailDto = Application.Features.Portfolios.GetPortfolioById.PortfolioDto;
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

public class CategoryDistributionItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
}

public class InvestmentsListViewModel
{
    public PaginatedList<InvestmentDto> Investments { get; set; }
    public List<string> Categories { get; set; }
    public List<PortfolioListDto> Portfolios { get; set; }
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
    public List<PortfolioListDto> Portfolios { get; set; } = new();
    public SystemSettingsViewModel Settings { get; set; } = new();
    public InvestmentsListViewModel ListViewModel { get; set; } = new();
    public PortfolioDetailDto? SelectedPortfolio { get; set; }
    public List<CategoryDistributionItem> CategoryDistribution { get; set; } = new();

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
                
            Portfolios = portfoliosResult.Value.ToList();

            // Get investments for calculating distributions
            var allInvestments = allInvestmentsResult.Value.Items.ToList();

            // Load selected portfolio details if a portfolio is selected
            if (PortfolioId.HasValue)
            {
                var portfolioResult = await _mediator.Send(new GetPortfolioByIdRequest { Id = PortfolioId.Value });
                if (portfolioResult.IsSuccess)
                {
                    SelectedPortfolio = new PortfolioDetailDto
                    {
                        Id = portfolioResult.Value.Id,
                        Name = portfolioResult.Value.Name,
                        Description = portfolioResult.Value.Description,
                        InitialValue = portfolioResult.Value.InitialValue,
                        TotalValue = portfolioResult.Value.TotalValue,
                        TotalInvestment = portfolioResult.Value.TotalInvestment,
                        UnrealizedGainLoss = portfolioResult.Value.UnrealizedGainLoss,
                        ReturnPercentage = portfolioResult.Value.ReturnPercentage,
                        CreatedOn = portfolioResult.Value.CreatedOn,
                        CreatedBy = portfolioResult.Value.CreatedBy,
                        ModifiedOn = portfolioResult.Value.ModifiedOn,
                        ModifiedBy = portfolioResult.Value.ModifiedBy,
                        IsActive = portfolioResult.Value.IsActive,
                        IsDelete = portfolioResult.Value.IsDelete
                    };
                    
                    // Get investments for this portfolio
                    var portfolioInvestments = allInvestments
                        .Where(i => i.PortfolioId == PortfolioId.Value)
                        .ToList();
                        
                    // Calculate category distribution
                    if (portfolioInvestments.Any())
                    {
                        var totalValue = portfolioInvestments.Sum(i => i.CurrentValue);
                        
                        CategoryDistribution = portfolioInvestments
                            .GroupBy(i => i.CategoryName ?? "Uncategorized")
                            .Select(g => new CategoryDistributionItem 
                            {
                                Name = g.Key,
                                Value = g.Sum(i => i.CurrentValue),
                                Percentage = totalValue > 0 ? (g.Sum(i => i.CurrentValue) / totalValue) * 100 : 0
                            })
                            .OrderByDescending(i => i.Value)
                            .ToList();
                    }
                }
            }
            else
            {
                // Create aggregate metrics for all portfolios combined
                var totalValue = allInvestments.Sum(i => i.CurrentValue);
                var totalInvestment = allInvestments.Sum(i => i.TotalInvestment);
                var unrealizedGainLoss = totalValue - totalInvestment;
                var returnPercentage = totalInvestment > 0 
                    ? (unrealizedGainLoss / totalInvestment) * 100
                    : 0;
                
                // Create an aggregate portfolio summary
                SelectedPortfolio = new PortfolioDetailDto
                {
                    Id = 0,
                    Name = "All Portfolios",
                    Description = "Combined summary of all portfolios",
                    InitialValue = totalInvestment,
                    TotalValue = totalValue,
                    TotalInvestment = totalInvestment,
                    UnrealizedGainLoss = unrealizedGainLoss,
                    ReturnPercentage = returnPercentage,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = 1,
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = 1,
                    IsActive = true,
                    IsDelete = false
                };
                
                // Create category distribution for all investments
                if (allInvestments.Any())
                {
                    CategoryDistribution = allInvestments
                        .GroupBy(i => i.CategoryName ?? "Uncategorized")
                        .Select(g => new CategoryDistributionItem 
                        {
                            Name = g.Key,
                            Value = g.Sum(i => i.CurrentValue),
                            Percentage = totalValue > 0 ? (g.Sum(i => i.CurrentValue) / totalValue) * 100 : 0
                        })
                        .OrderByDescending(i => i.Value)
                        .ToList();
                }
            }

            // Fetch paginated and filtered investments
            var request = new GetAllInvestmentsRequest
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                PortfolioId = PortfolioId,
                Category = Category,
                SearchText = SearchText,
                SortBy = SortBy,
                SortOrder = SortOrder
            };

            var result = await _mediator.Send(request);
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to fetch investments with filters");
                return StatusCode(500, "Failed to fetch investment data");
            }

            Investments = result.Value;

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