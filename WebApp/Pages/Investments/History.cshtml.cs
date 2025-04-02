using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Investments.GetInvestmentById;
using Application.Features.Investments.GetInvestmentHistory;
using Application.Features.Investments.RecordValueUpdate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Investments;

public class HistoryModel : PageModel
{
    private readonly IMediator _mediator;

    public InvestmentResponse Investment { get; set; } = null!;
    public List<InvestmentHistoryDto> InvestmentHistories { get; set; } = new();
    public List<Month> Months { get; set; } = new();
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }

    [BindProperty]
    public RecordValueUpdateRequest UpdateModel { get; set; } = new();

    public HistoryModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(int? id, int? month, int? year)
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

        Investment = result.Value;
        UpdateModel = new RecordValueUpdateRequest { InvestmentId = id.Value };

        // Get investment history
        await LoadHistoryData(id.Value, month, year);

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
            await LoadHistoryData(id.Value, null, null);
            return Page();
        }

        // Create a new request with the correct ID
        var updateRequest = new RecordValueUpdateRequest
        {
            InvestmentId = id.Value,
            NewValue = UpdateModel.NewValue,
            Note = UpdateModel.Note
        };

        // Record the new value
        var result = await _mediator.Send(updateRequest);
        
        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            
            await LoadHistoryData(id.Value, null, null);
            return Page();
        }

        // Reload the page with updated data
        return RedirectToPage(new { id = id.Value });
    }

    private async Task LoadHistoryData(int investmentId, int? month, int? year)
    {
        var now = DateTime.UtcNow;
        CurrentMonth = month ?? now.Month;
        CurrentYear = year ?? now.Year;

        var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = investmentId });
        if (result.IsSuccess)
        {
            Investment = result.Value;
        }

        // Get history data
        var historyResult = await _mediator.Send(new GetInvestmentHistoryRequest { InvestmentId = investmentId });
        if (historyResult.IsSuccess)
        {
            InvestmentHistories = historyResult.Value
                .Where(h => h.RecordedDate.Month == CurrentMonth && h.RecordedDate.Year == CurrentYear)
                .OrderByDescending(h => h.RecordedDate)
                .ToList();
        }

        // Populate months
        Months = new List<Month>
        {
            new Month { Number = 1, Name = "January" },
            new Month { Number = 2, Name = "February" },
            new Month { Number = 3, Name = "March" },
            new Month { Number = 4, Name = "April" },
            new Month { Number = 5, Name = "May" },
            new Month { Number = 6, Name = "June" },
            new Month { Number = 7, Name = "July" },
            new Month { Number = 8, Name = "August" },
            new Month { Number = 9, Name = "September" },
            new Month { Number = 10, Name = "October" },
            new Month { Number = 11, Name = "November" },
            new Month { Number = 12, Name = "December" }
        };
    }
}

public class Month
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
} 