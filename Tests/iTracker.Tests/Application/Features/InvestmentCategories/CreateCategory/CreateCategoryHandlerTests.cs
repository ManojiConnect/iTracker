using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Features.InvestmentCategories.CreateCategory;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using iTracker.Tests.Common.Extensions;
using iTracker.Tests.Common.Helpers;

namespace iTracker.Tests.Application.Features.InvestmentCategories.CreateCategory;

public class CreateCategoryHandlerTests
{
    private readonly Mock<IContext> _contextMock;
    private readonly Mock<DbSet<InvestmentCategory>> _categoriesDbSetMock;
    private readonly CreateCategoryHandler _handler;
    private readonly List<InvestmentCategory> _categories;

    public CreateCategoryHandlerTests()
    {
        _contextMock = new Mock<IContext>();
        _categories = new List<InvestmentCategory>();
        _categoriesDbSetMock = DbSetMockHelper.CreateDbSetMock<InvestmentCategory>();
        _categoriesDbSetMock.SetupData(_categories);

        _categoriesDbSetMock.Setup(d => d.Add(It.IsAny<InvestmentCategory>()))
            .Callback<InvestmentCategory>(entity =>
            {
                entity.Id = _categories.Count + 1;
                _categories.Add(entity);
            })
            .Returns((InvestmentCategory entity) => new TestEntityEntry<InvestmentCategory>(entity));

        _contextMock.Setup(x => x.InvestmentCategories).Returns(_categoriesDbSetMock.Object);
        _handler = new CreateCategoryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateCategoryRequest
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);
        _categoriesDbSetMock.Verify(m => m.Add(
            It.Is<InvestmentCategory>(c => 
                c.Name == command.Name && 
                c.Description == command.Description &&
                !c.IsDelete &&
                c.IsActive)), 
            Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var existingCategory = new InvestmentCategory
        {
            Id = 1,
            Name = "Existing Category",
            IsDelete = false,
            IsActive = true
        };
        _categories.Add(existingCategory);
        _categoriesDbSetMock.SetupData(_categories);

        var command = new CreateCategoryRequest
        {
            Name = "Existing Category",
            Description = "Test Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category with this name already exists", result.Errors);
        _categoriesDbSetMock.Verify(m => m.Add(It.IsAny<InvestmentCategory>()), Times.Never);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithMissingRequiredFields_ReturnsFailure(string name)
    {
        // Arrange
        var command = new CreateCategoryRequest
        {
            Name = name,
            Description = "Test Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name is required", result.Errors);
        _categoriesDbSetMock.Verify(m => m.Add(It.IsAny<InvestmentCategory>()), Times.Never);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMaxLengthExceeded_ReturnsFailure()
    {
        // Arrange
        var longName = new string('a', 101);
        var command = new CreateCategoryRequest
        {
            Name = longName,
            Description = "Test Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name cannot exceed 100 characters", result.Errors);
        _categoriesDbSetMock.Verify(m => m.Add(It.IsAny<InvestmentCategory>()), Times.Never);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        // Arrange
        var command = new CreateCategoryRequest
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to create category", result.Errors);
        _categoriesDbSetMock.Verify(m => m.Add(
            It.Is<InvestmentCategory>(c => 
                c.Name == command.Name && 
                c.Description == command.Description)), 
            Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}