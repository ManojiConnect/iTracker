using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Features.Investments.RecordValueUpdate;
using Ardalis.Result;
using Domain.Entities;
using iTracker.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace iTracker.Tests.Application.Features.Investments.RecordValueUpdate;

public class RecordValueUpdateHandlerTests : TestBase
{
    private readonly Mock<IContext> _contextMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;
    private readonly RecordValueUpdateHandler _handler;

    public RecordValueUpdateHandlerTests()
    {
        _contextMock = new Mock<IContext>();
        _settingsServiceMock = new Mock<ISettingsService>();
        _handler = new RecordValueUpdateHandler(_contextMock.Object, _settingsServiceMock.Object);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddScoped(_ => _contextMock.Object);
        services.AddScoped(_ => _settingsServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithSimpleCalculation_CalculatesCorrectReturnPercentage()
    {
        // Arrange
        var investment = new Investment
        {
            Id = 1,
            TotalInvestment = 1000m,
            CurrentValue = 1200m,
            CreatedOn = DateTime.UtcNow.AddYears(-1)
        };

        var dbSetMock = new Mock<DbSet<Investment>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(investment);

        _contextMock.Setup(x => x.Investments)
            .Returns(dbSetMock.Object);

        _settingsServiceMock.Setup(x => x.GetAllSettingsAsync())
            .ReturnsAsync(new SystemSettings { PerformanceCalculationMethod = "simple" });

        var request = new RecordValueUpdateRequest
        {
            InvestmentId = 1,
            NewValue = 1200m
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.2m, investment.ReturnPercentage); // (1200 - 1000) / 1000 = 0.2
    }

    [Fact]
    public async Task Handle_WithAnnualizedCalculation_CalculatesCorrectReturnPercentage()
    {
        // Arrange
        var investment = new Investment
        {
            Id = 1,
            TotalInvestment = 1000m,
            CurrentValue = 1200m,
            CreatedOn = DateTime.UtcNow.AddYears(-2)
        };

        var dbSetMock = new Mock<DbSet<Investment>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(investment);

        _contextMock.Setup(x => x.Investments)
            .Returns(dbSetMock.Object);

        _settingsServiceMock.Setup(x => x.GetAllSettingsAsync())
            .ReturnsAsync(new SystemSettings { PerformanceCalculationMethod = "annualized" });

        var request = new RecordValueUpdateRequest
        {
            InvestmentId = 1,
            NewValue = 1200m
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // For 2 years: (1 + 0.2)^(1/2) - 1 ≈ 0.0954
        Assert.Equal(0.0954m, Math.Round(investment.ReturnPercentage, 4));
    }

    [Fact]
    public async Task Handle_WithZeroInvestment_ReturnsZeroPercentage()
    {
        // Arrange
        var investment = new Investment
        {
            Id = 1,
            TotalInvestment = 0m,
            CurrentValue = 1000m
        };

        var dbSetMock = new Mock<DbSet<Investment>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(investment);

        _contextMock.Setup(x => x.Investments)
            .Returns(dbSetMock.Object);

        var request = new RecordValueUpdateRequest
        {
            InvestmentId = 1,
            NewValue = 1000m
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0m, investment.ReturnPercentage);
    }

    [Fact]
    public async Task Handle_WithLessThanYear_DefaultsToSimpleCalculation()
    {
        // Arrange
        var investment = new Investment
        {
            Id = 1,
            TotalInvestment = 1000m,
            CurrentValue = 1200m,
            CreatedOn = DateTime.UtcNow.AddMonths(-6)
        };

        var dbSetMock = new Mock<DbSet<Investment>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(investment);

        _contextMock.Setup(x => x.Investments)
            .Returns(dbSetMock.Object);

        _settingsServiceMock.Setup(x => x.GetAllSettingsAsync())
            .ReturnsAsync(new SystemSettings { PerformanceCalculationMethod = "annualized" });

        var request = new RecordValueUpdateRequest
        {
            InvestmentId = 1,
            NewValue = 1200m
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.2m, investment.ReturnPercentage); // Should use simple calculation for < 1 year
    }
} 