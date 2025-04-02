using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.InvestmentCategories.GetAllCategories;
using Application.Features.Investments.GetInvestmentById;
using Application.Features.Investments.UpdateInvestment;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages.Investments;

public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public UpdateInvestmentRequest Investment { get; set; } = new()
    {
        Id = 0,
        Name = string.Empty,
        CategoryId = 0,
        TotalInvestment = 0,
        CurrentValue = 0,
        PurchaseDate = DateTime.Today
    };

    public InvestmentResponse CurrentInvestment { get; set; } = default!;

    public EditModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id.Value });
        
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        CurrentInvestment = result.Value;
        
        // Extract category ID from category name (format is typically "ID - Name")
        int categoryId = 0;
        string categoryName = CurrentInvestment.CategoryName;
        if (categoryName.Contains('-'))
        {
            var categoryIdText = categoryName.Split('-')[0].Trim();
            int.TryParse(categoryIdText, out categoryId);
        }

        Investment = new UpdateInvestmentRequest
        {
            Id = CurrentInvestment.Id,
            Name = CurrentInvestment.Name,
            CategoryId = categoryId,
            TotalInvestment = CurrentInvestment.TotalInvestment,
            CurrentValue = CurrentInvestment.CurrentValue,
            PurchaseDate = CurrentInvestment.PurchaseDate
        };

        await LoadCategoriesSelectList();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id.Value });
            if (result.IsSuccess)
            {
                CurrentInvestment = result.Value;
            }
            
            await LoadCategoriesSelectList();
            return Page();
        }

        // If IDs don't match, create a new request with the correct ID
        if (Investment.Id != id.Value)
        {
            Investment = new UpdateInvestmentRequest
            {
                Id = id.Value,
                Name = Investment.Name,
                CategoryId = Investment.CategoryId,
                TotalInvestment = Investment.TotalInvestment,
                CurrentValue = Investment.CurrentValue,
                PurchaseDate = Investment.PurchaseDate
            };
        }
        
        var updateResult = await _mediator.Send(Investment);
        
        if (!updateResult.IsSuccess)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            
            var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id.Value });
            if (result.IsSuccess)
            {
                CurrentInvestment = result.Value;
            }
            
            await LoadCategoriesSelectList();
            return Page();
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesSelectList()
    {
        var categoriesResult = await _mediator.Send(new GetAllCategoriesRequest());
        if (categoriesResult.IsSuccess)
        {
            ViewData["Categories"] = new SelectList(
                categoriesResult.Value,
                "Id",
                "Name"
            );
        }
    }
} 