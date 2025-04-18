using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Application.Common.Interfaces;
using Application.Features.Investments.Common;
using System.Globalization;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Data;

namespace Application.Features.Investments.Commands.ImportInvestments;

public class ImportInvestmentsCommandHandler : IRequestHandler<ImportInvestmentsCommand, Result<List<string>>>
{
    private readonly IContext _context;
    private readonly ILogger<ImportInvestmentsCommandHandler> _logger;

    public ImportInvestmentsCommandHandler(
        IContext context,
        ILogger<ImportInvestmentsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<string>>> Handle(ImportInvestmentsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<string>();
            using var reader = new StringReader(request.CsvContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            });

            csv.Context.RegisterClassMap<ImportInvestmentDtoMap>();
            
            // Read the header
            await csv.ReadAsync();
            csv.ReadHeader();
            
            var records = new List<ImportInvestmentDto>();
            var rowIndex = 0;
            
            // Read the data rows
            while (await csv.ReadAsync())
            {
                try
                {
                    var record = new ImportInvestmentDto
                    {
                        Name = csv.GetField("Name"),
                        Category = csv.GetField("Category"),
                        TotalInvestment = decimal.Parse(csv.GetField("TotalInvestment"), CultureInfo.InvariantCulture),
                        CurrentValue = decimal.Parse(csv.GetField("CurrentValue"), CultureInfo.InvariantCulture),
                        PurchaseDate = DateTime.Parse(csv.GetField("PurchaseDate"), CultureInfo.InvariantCulture)
                    };
                    
                    // Only add the record if it's selected or if no rows are selected (import all)
                    if (request.SelectedRows == null || !request.SelectedRows.Any() || request.SelectedRows.Contains(rowIndex))
                    {
                        records.Add(record);
                    }
                    rowIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing CSV row {RowIndex}", rowIndex);
                    results.Add($"Failed to parse row {rowIndex}: {ex.Message}");
                    rowIndex++;
                }
            }

            foreach (var record in records)
            {
                try
                {
                    // Find or create category
                    var category = await _context.InvestmentCategories
                        .FirstOrDefaultAsync(c => c.Name == record.Category && !c.IsDelete, cancellationToken);

                    if (category == null)
                    {
                        category = new InvestmentCategory
                        {
                            Name = record.Category,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            ModifiedBy = 1,
                            ModifiedOn = DateTime.UtcNow,
                            IsActive = true,
                            IsDelete = false
                        };
                        _context.InvestmentCategories.Add(category);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    var investment = new Investment
                    {
                        Name = record.Name,
                        CategoryId = category.Id,
                        PortfolioId = request.PortfolioId,
                        TotalInvestment = record.TotalInvestment,
                        CurrentValue = record.CurrentValue,
                        PurchaseDate = record.PurchaseDate,
                        CreatedBy = 1,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = 1,
                        ModifiedOn = DateTime.UtcNow,
                        IsActive = true,
                        IsDelete = false
                    };

                    // Calculate gains
                    investment.UnrealizedGainLoss = investment.CurrentValue - investment.TotalInvestment;
                    investment.ReturnPercentage = investment.TotalInvestment > 0 
                        ? (investment.UnrealizedGainLoss / investment.TotalInvestment) * 100
                        : 0;

                    _context.Investments.Add(investment);
                    results.Add($"Successfully imported investment: {record.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing investment {Name}", record.Name);
                    results.Add($"Failed to import investment {record.Name}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<string>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV import");
            return Result<List<string>>.Error($"Failed to process CSV: {ex.Message}");
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

        throw new FormatException($"Unable to parse date: {text}");
    }
} 