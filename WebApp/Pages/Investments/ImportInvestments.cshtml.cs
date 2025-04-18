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
using Application.Features.Investments.Commands.ImportInvestments;
using WebApp.Models;
using System.Globalization;
using Application.Common.Models;
using Application.Features.Portfolios.GetPortfolioById;
using Application.Features.Portfolios.GetPortfolioInvestments;
using Application.Common.Interfaces;
using System.Text;

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
    public int PortfolioId { get; set; }

    [BindProperty]
    public IFormFile? File { get; set; }

    [BindProperty]
    public bool HasHeaders { get; set; } = true;

    [BindProperty]
    public bool IsPreview { get; set; }

    [BindProperty]
    public List<int> SelectedRecords { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CsvContent { get; set; }

    public List<Application.Features.Portfolios.GetAllPortfolios.PortfolioDto> Portfolios { get; set; } = new();
    public List<ImportInvestmentDto> PreviewRecords { get; set; } = new();
    public List<ImportResult> ImportResults { get; set; } = new();
    public List<int> DuplicateRecords { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetAllPortfoliosRequest());
        if (result.IsSuccess)
        {
            Portfolios = result.Value?.ToList() ?? new List<Application.Features.Portfolios.GetAllPortfolios.PortfolioDto>();
        }
        return Page();
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var template = new List<ImportInvestmentDto>
        {
            new ImportInvestmentDto
            {
                Name = "Example Investment",
                Category = "Stocks",
                TotalInvestment = 1000.00m,
                CurrentValue = 1100.00m,
                PurchaseDate = DateTime.Now
            }
        };

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        csv.Context.RegisterClassMap<ImportInvestmentDtoMap>();
        csv.WriteRecords(template);

        writer.Flush();
        memoryStream.Position = 0;

        return File(memoryStream.ToArray(), "text/csv", "investment_import_template.csv");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (IsPreview)
        {
            if (File == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to import.");
                return await OnGetAsync();
            }

            if (!File.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Please select a CSV file.");
                return await OnGetAsync();
            }

            // Load preview records
            await LoadPreviewRecords();
            return Page();
        }
        else
        {
            // Import selected records
            if (SelectedRecords == null || !SelectedRecords.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one record to import.");
                return await OnGetAsync();
            }

            // Get the CSV content from the hidden field or form data
            var csvContent = CsvContent;
            if (string.IsNullOrEmpty(csvContent))
            {
                csvContent = Request.Form["CsvContent"].ToString();
            }
            
            if (string.IsNullOrEmpty(csvContent))
            {
                ModelState.AddModelError(string.Empty, "CSV content is missing. Please try again.");
                return await OnGetAsync();
            }

            var command = new ImportInvestmentsCommand 
            { 
                CsvContent = csvContent,
                PortfolioId = PortfolioId,
                SelectedRows = SelectedRecords
            };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                ImportResults = result.Value?.Select((message, index) => 
                    new ImportResult 
                    { 
                        RowNumber = index + 1,
                        Name = ExtractNameFromMessage(message),
                        Success = message.StartsWith("Successfully"),
                        Message = message
                    }).ToList() ?? new List<ImportResult>();
                
                // Clear the preview records after successful import
                PreviewRecords.Clear();
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Failed to import investments. Please check the file format.");
            return await OnGetAsync();
        }
    }

    private async Task LoadPreviewRecords()
    {
        if (File == null || File.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a file to import.");
            return;
        }

        if (PortfolioId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a portfolio.");
            return;
        }

        // Get existing investments for the portfolio
        var existingInvestmentsRequest = new GetPortfolioInvestmentsRequest { PortfolioId = PortfolioId };
        var existingInvestmentsResult = await _mediator.Send(existingInvestmentsRequest);
        if (existingInvestmentsResult.IsSuccess)
        {
            var existingInvestments = existingInvestmentsResult.Value?.ToList() ?? new List<InvestmentDto>();
            _logger.LogInformation("Found {Count} existing investments for portfolio {PortfolioId}", 
                existingInvestments.Count, PortfolioId);

            // Store the CSV content for later use
            using var stream = File.OpenReadStream();
            using var reader = new StreamReader(stream);
            CsvContent = await reader.ReadToEndAsync();

            using var csvReader = new StringReader(CsvContent);
            using var csv = new CsvReader(csvReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = HasHeaders,
                MissingFieldFound = null,
                HeaderValidated = null
            });

            csv.Context.RegisterClassMap<ImportInvestmentDtoMap>();
            var records = new List<ImportInvestmentDto>();
            var rowNumber = HasHeaders ? 1 : 0;

            try
            {
                // Read the header if the file has headers
                if (HasHeaders)
                {
                    await csv.ReadAsync();
                    csv.ReadHeader();
                }

                // Read the data rows
                while (await csv.ReadAsync())
                {
                    rowNumber++;
                    try
                    {
                        var record = new ImportInvestmentDto
                        {
                            Name = csv.GetField("Name"),
                            Category = csv.GetField("Category"),
                            TotalInvestment = decimal.Parse(csv.GetField("TotalInvestment"), CultureInfo.InvariantCulture),
                            CurrentValue = decimal.Parse(csv.GetField("CurrentValue"), CultureInfo.InvariantCulture),
                            PurchaseDate = DateTime.Parse(csv.GetField("PurchaseDate"), CultureInfo.InvariantCulture),
                            PortfolioId = PortfolioId
                        };

                        records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing row {RowNumber}", rowNumber);
                        ModelState.AddModelError(string.Empty, $"Error parsing row {rowNumber}: {ex.Message}");
                    }
                }

                // Check for duplicates
                DuplicateRecords = new List<int>();
                var seenInvestments = new Dictionary<string, int>();
                
                _logger.LogInformation($"Starting duplicate detection for {records.Count} records in portfolio {PortfolioId}");
                
                // First check against existing investments in the database
                for (var i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    var key = record.GetDuplicateKey();
                    
                    _logger.LogInformation($"Record {i}: Portfolio {record.PortfolioId}, {record.Name}, {record.TotalInvestment}");
                    _logger.LogInformation($"Generated key: {key}");
                    
                    // Check against existing investments
                    var existingDuplicate = existingInvestments.FirstOrDefault(e => 
                        e.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase) && 
                        e.TotalInvestment == record.TotalInvestment);
                        
                    if (existingDuplicate != null)
                    {
                        _logger.LogInformation($"Found duplicate with existing investment: {existingDuplicate.Name}");
                        DuplicateRecords.Add(i);
                        continue;
                    }
                    
                    // Then check against other records in the import
                    if (seenInvestments.ContainsKey(key))
                    {
                        _logger.LogInformation($"Found duplicate at index {i} matching index {seenInvestments[key]}");
                        // Add both the current record and the previously seen record as duplicates
                        if (!DuplicateRecords.Contains(i))
                        {
                            DuplicateRecords.Add(i);
                        }
                        if (!DuplicateRecords.Contains(seenInvestments[key]))
                        {
                            DuplicateRecords.Add(seenInvestments[key]);
                        }
                    }
                    else
                    {
                        seenInvestments[key] = i;
                    }
                }

                // Log duplicate detection results for debugging
                _logger.LogInformation($"Found {DuplicateRecords.Count} duplicate records out of {records.Count} total records in portfolio {PortfolioId}");
                foreach (var index in DuplicateRecords)
                {
                    var record = records[index];
                    _logger.LogInformation($"Duplicate record at index {index}: Portfolio {record.PortfolioId}, {record.Name}, {record.TotalInvestment}");
                }

                PreviewRecords = records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading preview records");
                ModelState.AddModelError(string.Empty, $"Error loading preview records: {ex.Message}");
            }
        }
    }

    private string ExtractNameFromMessage(string message)
    {
        if (message.StartsWith("Successfully imported investment:"))
        {
            return message.Replace("Successfully imported investment:", "").Trim();
        }
        else if (message.Contains("Failed to import investment"))
        {
            var parts = message.Split(':');
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }
        }
        return "Unknown";
    }
}

public class ImportInvestmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int PortfolioId { get; set; }

    public string GetDuplicateKey()
    {
        // Normalize the values to ensure consistent comparison
        // Use Name instead of Category for duplicate detection
        var normalizedName = Name.Trim().ToLowerInvariant();
        var normalizedAmount = TotalInvestment.ToString("F2", CultureInfo.InvariantCulture);
        
        return $"{PortfolioId}|{normalizedName}|{normalizedAmount}";
    }
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
            if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, 
            DateTimeStyles.None, out DateTime defaultResult))
        {
            return defaultResult;
        }

        throw new FormatException($"Unable to parse date: {text}. Expected format: dd/MM/yyyy");
    }
}

public class ImportResult
{
    public int RowNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
} 