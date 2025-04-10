using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Investments.GetInvestmentById;
using Application.Features.Investments.GetInvestmentHistory;
using Application.Features.Investments.RecordValueUpdate;
using Application.Features.Investments.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages.Investments;

public class HistoryModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IApplicationSettingsService _settingsService;

    public InvestmentDto Investment { get; set; } = null!;
    public List<InvestmentHistoryDto> InvestmentHistories { get; set; } = new();
    public List<Month> Months { get; set; } = new();
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }

    [BindProperty]
    public RecordValueUpdateRequest UpdateModel { get; set; } = new();

    public HistoryModel(IMediator mediator, IApplicationSettingsService settingsService)
    {
        _mediator = mediator;
        _settingsService = settingsService;
    }

    public string FormatNumber(decimal number, int? decimalPlaces = null)
    {
        return _settingsService.FormatNumber(number, decimalPlaces);
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
            return Page();
        }

        var result = await _mediator.Send(UpdateModel);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to update investment value");
            return Page();
        }

        return RedirectToPage(new { id });
    }

    private async Task LoadHistoryData(int id, int? month, int? year)
    {
        var now = DateTime.UtcNow;
        CurrentMonth = month ?? now.Month;
        CurrentYear = year ?? now.Year;

        var startDate = new DateTime(CurrentYear, CurrentMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var historyResult = await _mediator.Send(new GetInvestmentHistoryRequest
        {
            InvestmentId = id,
            StartDate = startDate,
            EndDate = endDate
        });

        if (historyResult.IsSuccess)
        {
            InvestmentHistories = historyResult.Value;
        }

        // Generate list of months for the year
        Months = Enumerable.Range(1, 12)
            .Select(m => new Month
            {
                Number = m,
                Name = new DateTime(CurrentYear, m, 1).ToString("MMMM"),
                IsCurrent = m == CurrentMonth
            })
            .ToList();
    }
}

public class Month
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
} 