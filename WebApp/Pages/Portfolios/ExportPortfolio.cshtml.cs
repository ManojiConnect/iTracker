using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Application.Features.Portfolios.GetAllPortfolios;
using Application.Features.Investments.Common;
using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;

namespace WebApp.Pages.Portfolios;

public class ExportPortfolioModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExportPortfolioModel> _logger;

    public ExportPortfolioModel(
        IMediator mediator,
        ILogger<ExportPortfolioModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public int? PortfolioId { get; set; }

    [BindProperty]
    public bool IncludeHeaders { get; set; } = true;

    [BindProperty]
    public bool IncludeNotes { get; set; } = true;

    public List<Application.Features.Portfolios.GetAllPortfolios.PortfolioDto> Portfolios { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var portfoliosQuery = new GetAllPortfoliosRequest();
            var result = await _mediator.Send(portfoliosQuery);
            
            if (result.IsSuccess)
            {
                Portfolios = result.Value.ToList();
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolios for export");
            TempData["ErrorMessage"] = "An error occurred while loading portfolios.";
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!PortfolioId.HasValue)
            {
                ModelState.AddModelError("PortfolioId", "Please select a portfolio to export.");
                return await OnGetAsync();
            }

            var portfolioResult = await _mediator.Send(new GetPortfolioByIdRequest { Id = PortfolioId.Value });
            if (!portfolioResult.IsSuccess)
            {
                TempData["ErrorMessage"] = "Portfolio not found.";
                return RedirectToPage();
            }

            var portfolio = portfolioResult.Value;

            var investmentsResult = await _mediator.Send(new GetPortfolioInvestmentsRequest { PortfolioId = PortfolioId.Value });
            if (!investmentsResult.IsSuccess)
            {
                TempData["ErrorMessage"] = "Failed to load investments.";
                return RedirectToPage();
            }

            var investments = investmentsResult.Value;

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

            try
            {
                // Write portfolio information
                csv.WriteField("Portfolio Name");
                csv.WriteField(portfolio.Name);
                await csv.NextRecordAsync();
                
                csv.WriteField("Description");
                csv.WriteField(portfolio.Description);
                await csv.NextRecordAsync();
                
                await csv.NextRecordAsync(); // Empty line for separation

                if (IncludeHeaders)
                {
                    csv.WriteHeader<InvestmentExportDto>();
                    await csv.NextRecordAsync();
                }

                foreach (var investment in investments)
                {
                    var exportDto = new InvestmentExportDto
                    {
                        Name = investment.Name,
                        Category = investment.CategoryName,
                        Symbol = investment.Symbol,
                        TotalInvestment = investment.TotalInvestment,
                        CurrentValue = investment.CurrentValue,
                        PurchaseDate = investment.PurchaseDate,
                        Notes = IncludeNotes ? investment.Notes : null
                    };

                    csv.WriteRecord(exportDto);
                    await csv.NextRecordAsync();
                }

                await writer.FlushAsync();
                memoryStream.Position = 0;
                var fileName = $"portfolio_{portfolio.Name?.ToLower().Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";

                return File(memoryStream, "text/csv", fileName);
            }
            finally
            {
                await writer.DisposeAsync();
                csv.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting portfolio");
            TempData["ErrorMessage"] = "An error occurred while exporting the portfolio.";
            return RedirectToPage();
        }
    }
}

public class InvestmentExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
} 