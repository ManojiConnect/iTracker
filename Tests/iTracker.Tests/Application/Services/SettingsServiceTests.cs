using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using iTracker.Tests.Common.Helpers;
using System.Linq.Expressions;
using System.Threading;

namespace iTracker.Tests.Application.Services;

public class SettingsServiceTests
{
    private readonly Mock<IContext> _contextMock;
    private Mock<DbSet<SystemSettings>> _settingsDbSetMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<SettingsService>> _loggerMock;
    private readonly SettingsService _settingsService;
    private const string SettingsCacheKey = "SystemSettings";

    public SettingsServiceTests()
    {
        _contextMock = new Mock<IContext>();
        _settingsDbSetMock = DbSetMockHelper.CreateDbSetMock<SystemSettings>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<SettingsService>>();
        _contextMock.Setup(c => c.SystemSettings).Returns(_settingsDbSetMock.Object);

        // Setup cache mock
        object? outValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        var cacheEntry = new Mock<ICacheEntry>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry.Object);

        _settingsService = new SettingsService(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllSettings_WhenNoSettings_ReturnsDefaultSettings()
    {
        // Arrange
        var settings = new List<SystemSettings>();
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.GetAllSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("$", result.CurrencySymbol);
        Assert.Equal(".", result.DecimalSeparator);
        Assert.Equal(",", result.ThousandsSeparator);
        Assert.Equal(2, result.DecimalPlaces);
        Assert.Equal("MM/dd/yyyy", result.DateFormat);
        Assert.Equal(4, result.FinancialYearStartMonth);
        Assert.Equal("simple", result.PerformanceCalculationMethod);
        Assert.Equal(30, result.SessionTimeoutMinutes);

        // Verify the default settings were added to the database
        _settingsDbSetMock.Verify(m => m.Add(It.IsAny<SystemSettings>()), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllSettings_WhenSettingsExist_ReturnsExistingSettings()
    {
        // Arrange
        var existingSettings = new SystemSettings
        {
            Id = 1,
            CurrencySymbol = "€",
            DecimalSeparator = ",",
            ThousandsSeparator = ".",
            DecimalPlaces = 3,
            DateFormat = "dd/MM/yyyy",
            FinancialYearStartMonth = 1,
            PerformanceCalculationMethod = "annualized",
            SessionTimeoutMinutes = 60
        };

        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.GetAllSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("€", result.CurrencySymbol);
        Assert.Equal(",", result.DecimalSeparator);
        Assert.Equal(".", result.ThousandsSeparator);
        Assert.Equal(3, result.DecimalPlaces);
        Assert.Equal("dd/MM/yyyy", result.DateFormat);
        Assert.Equal(1, result.FinancialYearStartMonth);
        Assert.Equal("annualized", result.PerformanceCalculationMethod);
        Assert.Equal(60, result.SessionTimeoutMinutes);

        // Verify no new settings were added
        _settingsDbSetMock.Verify(m => m.Add(It.IsAny<SystemSettings>()), Times.Never);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSetting_WithExistingProperty_ReturnsPropertyValue()
    {
        // Arrange
        var existingSettings = new SystemSettings
        {
            Id = 1,
            CurrencySymbol = "€",
            PerformanceCalculationMethod = "annualized"
        };

        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.GetSettingAsync("CurrencySymbol");

        // Assert
        Assert.Equal("€", result);
    }

    [Fact]
    public async Task GetSetting_WithNonExistingProperty_ReturnsEmptyString()
    {
        // Arrange
        var existingSettings = new SystemSettings { Id = 1 };
        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.GetSettingAsync("NonExistingProperty");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task UpdateSettings_WithValidSettings_UpdatesSuccessfully()
    {
        // Arrange
        var existingSettings = new SystemSettings { Id = 1 };
        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        var updatedSettings = new SystemSettings
        {
            Id = 1,
            CurrencySymbol = "£",
            DecimalSeparator = ".",
            ThousandsSeparator = ",",
            DecimalPlaces = 2,
            DateFormat = "dd/MM/yyyy",
            FinancialYearStartMonth = 4,
            PerformanceCalculationMethod = "simple",
            SessionTimeoutMinutes = 45
        };

        // Act
        var result = await _settingsService.UpdateSettingsAsync(updatedSettings);

        // Assert
        Assert.True(result);
        _settingsDbSetMock.Verify(m => m.Update(It.IsAny<SystemSettings>()), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.Remove(SettingsCacheKey), Times.Once);
    }

    [Fact]
    public async Task UpdateSetting_WithValidProperty_UpdatesSuccessfully()
    {
        // Arrange
        var existingSettings = new SystemSettings { Id = 1, CurrencySymbol = "$" };
        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.UpdateSettingAsync("CurrencySymbol", "€");

        // Assert
        Assert.True(result);
        Assert.Equal("€", existingSettings.CurrencySymbol);
        _settingsDbSetMock.Verify(m => m.Update(It.IsAny<SystemSettings>()), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.Remove(SettingsCacheKey), Times.Once);
    }

    [Fact]
    public async Task UpdateSetting_WithInvalidProperty_ReturnsFalse()
    {
        // Arrange
        var existingSettings = new SystemSettings { Id = 1, CurrencySymbol = "$" };
        var settings = new List<SystemSettings> { existingSettings };
        _settingsDbSetMock.SetupData(settings);

        // Act
        var result = await _settingsService.UpdateSettingAsync("NonExistentProperty", "test");

        // Assert
        Assert.False(result);
        _settingsDbSetMock.Verify(m => m.Update(It.IsAny<SystemSettings>()), Times.Never);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.Remove(SettingsCacheKey), Times.Never);
    }
} 