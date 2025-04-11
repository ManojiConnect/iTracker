using Application.Features.Investments.CreateInvestment;
using Application.Features.InvestmentCategories.GetAllCategories;
using Application.Features.Portfolios.GetAllPortfolios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WebApp.Pages.Investments;

public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateModel> _logger;

    [BindProperty]
    public CreateInvestmentRequest Investment { get; set; } = new()
    {
        Name = string.Empty,
        Symbol = string.Empty,
        PortfolioId = 0,
        CategoryId = 0,
        TotalInvestment = 0,
        CurrentValue = 0,
        PurchaseDate = DateTime.Today,
        PurchasePrice = 0,
        Notes = string.Empty
    };

    public CreateModel(IMediator mediator, ILogger<CreateModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int portfolioId)
    {
        Investment = new CreateInvestmentRequest
        {
            Name = string.Empty,
            Symbol = string.Empty,
            PortfolioId = portfolioId,
            CategoryId = 0,
            TotalInvestment = 0,
            CurrentValue = 0,
            PurchaseDate = DateTime.Today,
            PurchasePrice = 0,
            Notes = string.Empty
        };

        await LoadCategoriesDropdown();
        await LoadPortfoliosDropdown();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Investment creation started");
        _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);

        if (!ModelState.IsValid)
        {
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    _logger.LogError("Key: {Key}, Error: {Error}", modelStateKey, error.ErrorMessage);
                }
            }
            
            await LoadCategoriesDropdown();
            await LoadPortfoliosDropdown();
            return Page();
        }

        _logger.LogInformation("Investment data: {Investment}", JsonSerializer.Serialize(Investment));

        // Set TotalInvestment to PurchasePrice value
        Investment = Investment with
        {
            TotalInvestment = Investment.PurchasePrice
        };

        try
        {
            _logger.LogInformation("Sending create investment request to mediator");
            var result = await _mediator.Send(Investment);
            
            _logger.LogInformation("Mediator result: {Success}, {Errors}", 
                result.IsSuccess, 
                result.IsSuccess ? "No errors" : string.Join(", ", result.Errors));
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Investment created successfully with ID: {Id}", result.Value);
                return RedirectToPage("./Index", new { portfolioId = Investment.PortfolioId });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
                _logger.LogError("Error creating investment: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating investment");
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
        }

        await LoadCategoriesDropdown();
        await LoadPortfoliosDropdown();
        return Page();
    }

    private async Task LoadCategoriesDropdown()
    {
        var categoriesResult = await _mediator.Send(new GetAllCategoriesRequest());
        if (categoriesResult.IsSuccess)
        {
            ViewData["CategoryId"] = new SelectList(
                categoriesResult.Value,
                nameof(CategoryDto.Id),
                nameof(CategoryDto.Name)
            );
        }
    }

    private async Task LoadPortfoliosDropdown()
    {
        var portfoliosResult = await _mediator.Send(new GetAllPortfoliosRequest());
        if (portfoliosResult.IsSuccess)
        {
            ViewData["PortfolioId"] = new SelectList(
                portfoliosResult.Value,
                nameof(PortfolioDto.Id),
                nameof(PortfolioDto.Name)
            );
        }
    }
} 