using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

namespace WebApp.Pages.Investments;

public class DownloadTemplateModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public DownloadTemplateModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IActionResult OnGet()
    {
        try
        {
            // Create a memory stream for the CSV
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

            // Configure date format for UK/India (dd/MM/yyyy)
            csv.Context.RegisterClassMap<ImportInvestmentDtoMap>();

            // Write headers
            csv.WriteHeader<ImportInvestmentDto>();
            csv.NextRecord();

            // Write a sample record
            csv.WriteRecord(new ImportInvestmentDto
            {
                Name = "Sample Investment",
                Category = "Stocks",
                TotalInvestment = 1000.00m,
                CurrentValue = 1100.00m,
                PurchaseDate = DateTime.Now
            });

            // Flush and prepare for download
            writer.Flush();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "investment-import-template.csv");
        }
        catch (Exception)
        {
            return RedirectToPage("./ImportInvestments");
        }
    }
} 