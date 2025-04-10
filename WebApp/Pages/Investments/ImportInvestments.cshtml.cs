using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Application.Features.Investments.Common;
using Application.Features.Investments.CreateInvestment;
using Application.Features.Portfolios.GetAllPortfolios;
using Application.Features.InvestmentCategories.GetAllCategories;
using Ardalis.Result;

namespace WebApp.Pages.Investments;

public class ImportInvestmentsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportInvestmentsModel> _logger;

    public ImportInvestmentsModel(
        IMediator mediator,
        ILogger<ImportInvestmentsModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public int? PortfolioId { get; set; }

    [BindProperty]
    public new IFormFile File { get; set; }

    [BindProperty]
    public bool HasHeaders { get; set; } = true;

    public List<Application.Features.Portfolios.GetAllPortfolios.PortfolioDto> Portfolios { get; set; } = new();

    public List<ImportResult> ImportResults { get; set; } = new();

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
            _logger.LogError(ex, "Error loading portfolios for import");
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
                ModelState.AddModelError("PortfolioId", "Please select a portfolio to import into.");
                return await OnGetAsync();
            }

            if (File == null || File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to import.");
                return await OnGetAsync();
            }

            if (!File.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("File", "Please upload a CSV file.");
                return await OnGetAsync();
            }

            // Load all categories for lookup
            var categoriesResult = await _mediator.Send(new GetAllCategoriesRequest());
            if (!categoriesResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Failed to load categories.");
                return await OnGetAsync();
            }

            var categories = categoriesResult.Value.ToList();
            var categoryNameToId = categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

            using var reader = new StreamReader(File.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = HasHeaders,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                BadDataFound = null,
                MissingFieldFound = null
            });

            // Configure date format for UK/India (dd/MM/yyyy)
            csv.Context.RegisterClassMap<ImportInvestmentDtoMap>();

            var records = new List<ImportInvestmentDto>();
            await foreach (var record in csv.GetRecordsAsync<ImportInvestmentDto>())
            {
                if (string.IsNullOrWhiteSpace(record.Name))
                {
                    continue;
                }

                records.Add(record);
            }

            if (!records.Any())
            {
                ModelState.AddModelError("File", "No valid records found in the file.");
                return await OnGetAsync();
            }

            // Create investments one by one
            var rowNumber = 1;
            foreach (var record in records)
            {
                // Look up category ID by name
                if (!categoryNameToId.TryGetValue(record.Category, out int categoryId))
                {
                    ImportResults.Add(new ImportResult
                    {
                        RowNumber = rowNumber++,
                        Name = record.Name,
                        Success = false,
                        Message = $"Category '{record.Category}' not found. Please create this category first."
                    });
                    continue;
                }

                var request = new CreateInvestmentRequest
                {
                    PortfolioId = PortfolioId.Value,
                    Name = record.Name,
                    CategoryId = categoryId,
                    TotalInvestment = record.TotalInvestment,
                    CurrentValue = record.CurrentValue,
                    PurchaseDate = record.PurchaseDate
                };

                var result = await _mediator.Send(request);
                ImportResults.Add(new ImportResult
                {
                    RowNumber = rowNumber++,
                    Name = record.Name,
                    Success = result.IsSuccess,
                    Message = result.IsSuccess ? "Successfully imported" : result.Errors.FirstOrDefault() ?? "Unknown error"
                });
            }

            if (ImportResults.Any(r => r.Success))
            {
                TempData["SuccessMessage"] = $"Successfully imported {ImportResults.Count(r => r.Success)} investments.";
                return RedirectToPage("./Index");
            }

            return await OnGetAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing investments");
            ModelState.AddModelError(string.Empty, "An error occurred while importing investments.");
            return await OnGetAsync();
        }
    }
}

public class ImportInvestmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
}

public class ImportResult
{
    public int RowNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ImportInvestmentDtoMap : ClassMap<ImportInvestmentDto>
{
    public ImportInvestmentDtoMap()
    {
        Map(m => m.Name);
        Map(m => m.Category);
        Map(m => m.TotalInvestment);
        Map(m => m.CurrentValue);
        Map(m => m.PurchaseDate).TypeConverter<FlexibleDateConverter>();
    }
}

public class FlexibleDateConverter : CsvHelper.TypeConversion.DateTimeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return DateTime.MinValue;

        // Try parsing with different formats
        string[] formats = new[] { 
            "dd/MM/yyyy", 
            "dd/MM/yy", 
            "dd/MM/yy HH:mm", 
            "dd/MM/yy H:mm",
            "dd/MM/yy 0:00",
            "dd/MM/yy 00:00",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy H:mm"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(text, format, System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
        }

        // If all formats fail, try parsing with the default DateTime parser
        if (DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out DateTime defaultResult))
        {
            return defaultResult;
        }

        throw new FormatException($"Could not convert '{text}' to DateTime. Expected format: dd/MM/yyyy or dd/MM/yy");
    }
} 