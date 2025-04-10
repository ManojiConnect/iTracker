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
using Application.Features.Investments.Common;
using Application.Features.Portfolios.GetAllPortfolios;
using Application.Features.Portfolios.GetPortfolioInvestments;

namespace WebApp.Pages.Investments;

public class ExportInvestmentsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExportInvestmentsModel> _logger;

    public ExportInvestmentsModel(
        IMediator mediator,
        ILogger<ExportInvestmentsModel> logger)
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
                await LoadPortfoliosAsync();
                return Page();
            }

            var investmentsResult = await _mediator.Send(new GetPortfolioInvestmentsRequest { PortfolioId = PortfolioId.Value });
            if (!investmentsResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Failed to load investments.");
                await LoadPortfoliosAsync();
                return Page();
            }

            var investments = investmentsResult.Value;

            if (!investments.Any())
            {
                var selectedPortfolio = Portfolios.FirstOrDefault(p => p.Id == PortfolioId.Value);
                var portfolioName = selectedPortfolio?.Name ?? "Selected portfolio";
                ModelState.AddModelError(string.Empty, $"No investments found in {portfolioName}. Please add investments before exporting.");
                await LoadPortfoliosAsync();
                return Page();
            }

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

            // Configure date format for UK/India (dd/MM/yyyy)
            csv.Context.RegisterClassMap<InvestmentExportDtoMap>();

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
                    TotalInvestment = investment.TotalInvestment,
                    CurrentValue = investment.CurrentValue,
                    PurchaseDate = investment.PurchaseDate
                };

                csv.WriteRecord(exportDto);
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;

            var portfolio = Portfolios.FirstOrDefault(p => p.Id == PortfolioId.Value);
            var fileName = $"investments_{portfolio?.Name?.ToLower().Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";

            return File(memoryStream, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting investments");
            ModelState.AddModelError(string.Empty, "An error occurred while exporting investments.");
            await LoadPortfoliosAsync();
            return Page();
        }
    }

    private async Task LoadPortfoliosAsync()
    {
        var portfoliosQuery = new GetAllPortfoliosRequest();
        var result = await _mediator.Send(portfoliosQuery);
        
        if (result.IsSuccess)
        {
            Portfolios = result.Value.ToList();
        }
    }
}

public class InvestmentExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
}

public class InvestmentExportDtoMap : ClassMap<InvestmentExportDto>
{
    public InvestmentExportDtoMap()
    {
        Map(m => m.Name);
        Map(m => m.Category);
        Map(m => m.TotalInvestment);
        Map(m => m.CurrentValue);
        Map(m => m.PurchaseDate).TypeConverterOption.Format("dd/MM/yyyy");
    }
} 