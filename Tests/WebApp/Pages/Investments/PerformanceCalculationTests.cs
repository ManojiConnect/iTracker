using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Pages.Investments;
using Xunit;

namespace Tests.WebApp.Pages.Investments;

public class PerformanceCalculationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PerformanceCalculationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the database context with an in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RecordValueUpdate_WithSimpleCalculation_ShowsCorrectPerformance()
    {
        // Arrange
        // First, set the performance calculation method to simple
        await SetPerformanceCalculationMethod("simple");

        // Create a test investment
        var investment = await CreateTestInvestment(1000m, DateTime.UtcNow.AddYears(-1));

        // Act
        // Update the investment value
        var response = await _client.PostAsJsonAsync($"/api/investments/{investment.Id}/value-update", new
        {
            NewValue = 1200m,
            Note = "Test update"
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        // Get the updated investment details
        var detailsResponse = await _client.GetAsync($"/Investments/Details/{investment.Id}");
        Assert.True(detailsResponse.IsSuccessStatusCode);

        var content = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("20%", content); // Simple return should be 20%
    }

    [Fact]
    public async Task RecordValueUpdate_WithAnnualizedCalculation_ShowsCorrectPerformance()
    {
        // Arrange
        // Set the performance calculation method to annualized
        await SetPerformanceCalculationMethod("annualized");

        // Create a test investment
        var investment = await CreateTestInvestment(1000m, DateTime.UtcNow.AddYears(-2));

        // Act
        // Update the investment value
        var response = await _client.PostAsJsonAsync($"/api/investments/{investment.Id}/value-update", new
        {
            NewValue = 1200m,
            Note = "Test update"
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        // Get the updated investment details
        var detailsResponse = await _client.GetAsync($"/Investments/Details/{investment.Id}");
        Assert.True(detailsResponse.IsSuccessStatusCode);

        var content = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("9.54%", content); // Annualized return should be approximately 9.54%
    }

    private async Task SetPerformanceCalculationMethod(string method)
    {
        var settings = new SystemSettingsViewModel
        {
            PerformanceCalculationMethod = method
        };

        await _client.PostAsJsonAsync("/api/settings", settings);
    }

    private async Task<Investment> CreateTestInvestment(decimal initialValue, DateTime startDate)
    {
        var investment = new Investment
        {
            Name = "Test Investment",
            TotalInvestment = initialValue,
            CurrentValue = initialValue,
            CreatedOn = startDate
        };

        var response = await _client.PostAsJsonAsync("/api/investments", investment);
        Assert.True(response.IsSuccessStatusCode);

        return await response.Content.ReadFromJsonAsync<Investment>();
    }
} 