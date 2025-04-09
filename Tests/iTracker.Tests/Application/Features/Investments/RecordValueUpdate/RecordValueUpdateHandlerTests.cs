using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Features.Investments.RecordValueUpdate;
using Ardalis.Result;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using iTracker.Tests.Common;
using iTracker.Tests.Common.TestData;
using iTracker.Tests.Common.Extensions;

namespace iTracker.Tests.Application.Features.Investments.RecordValueUpdate;

public class RecordValueUpdateHandlerTests
{
    private readonly Mock<IContext> _contextMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;
    private readonly Mock<DbSet<Investment>> _investmentsDbSetMock;
    private readonly Mock<DbSet<InvestmentHistory>> _historiesDbSetMock;
    private readonly RecordValueUpdateHandler _handler;

    public RecordValueUpdateHandlerTests()
    {
        _contextMock = new Mock<IContext>();
        _settingsServiceMock = new Mock<ISettingsService>();
        
        // Setup test data
        var investment = new Investment
        {
            Id = 1,
            Name = "Test Investment",
            TotalInvestment = 800,
            CurrentValue = 1000,
            Portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                TotalInvestment = 1500,
                TotalValue = 2000
            }
        };

        var investments = new List<Investment> { investment }.AsQueryable();
        _investmentsDbSetMock = investments.BuildMock();
        _contextMock.Setup(x => x.Investments).Returns(_investmentsDbSetMock.Object);

        var histories = new List<InvestmentHistory>().AsQueryable();
        _historiesDbSetMock = histories.BuildMock();
        _contextMock.Setup(x => x.InvestmentHistories).Returns(_historiesDbSetMock.Object);

        _settingsServiceMock.Setup(x => x.GetAllSettingsAsync())
            .ReturnsAsync(new SystemSettings { PerformanceCalculationMethod = "simple" });

        _handler = new RecordValueUpdateHandler(_contextMock.Object, _settingsServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenInvestmentExists_ShouldUpdateValueAndCreateHistory()
    {
        // Arrange
        var command = new RecordValueUpdateRequest { InvestmentId = 1, NewValue = 1200 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify investment was updated
        var investment = _investmentsDbSetMock.Object.First();
        Assert.NotNull(investment);
        Assert.Equal(1200, investment.CurrentValue);
        Assert.Equal(50, investment.ReturnPercentage);
        
        // Verify history was created
        _historiesDbSetMock.Verify(m => m.Add(It.Is<InvestmentHistory>(h => 
            h.InvestmentId == 1 && 
            h.Value == 1200)), Times.Once);
            
        // Verify changes were saved
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvestmentDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var investments = new List<Investment>().AsQueryable();
        var emptyDbSetMock = investments.BuildMock();
        _contextMock.Setup(x => x.Investments).Returns(emptyDbSetMock.Object);

        var command = new RecordValueUpdateRequest { InvestmentId = 999, NewValue = 1200 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal("Investment not found", result.Errors.First());
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSimpleCalculation_CalculatesCorrectReturnPercentage()
    {
        // Arrange
        var command = new RecordValueUpdateRequest { InvestmentId = 1, NewValue = 1200 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var investment = _investmentsDbSetMock.Object.First();
        Assert.NotNull(investment);
        Assert.Equal(1200, investment.CurrentValue);
        Assert.Equal(50, investment.ReturnPercentage); // (1200 - 800) / 800 * 100
    }
} 