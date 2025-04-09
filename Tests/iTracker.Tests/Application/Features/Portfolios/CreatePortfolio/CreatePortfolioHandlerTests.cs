using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Features.Portfolios.CreatePortfolio;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using iTracker.Tests.Common;
using iTracker.Tests.Common.Helpers;
using Xunit;

namespace iTracker.Tests.Application.Features.Portfolios.CreatePortfolio
{
    public class CreatePortfolioHandlerTests : TestBase
    {
        private readonly Mock<IContext> _contextMock;
        private readonly Mock<DbSet<Portfolio>> _portfolioDbSetMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly CreatePortfolioHandler _handler;
        private readonly List<Portfolio> _portfolios;

        public CreatePortfolioHandlerTests()
        {
            _contextMock = new Mock<IContext>();
            _portfolios = new List<Portfolio>();
            _portfolioDbSetMock = DbSetMockHelper.CreateDbSetMock<Portfolio>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new CreatePortfolioHandler(_contextMock.Object, _currentUserServiceMock.Object);

            _portfolioDbSetMock.SetupData(_portfolios);
            _contextMock.Setup(x => x.Portfolios).Returns(_portfolioDbSetMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new CreatePortfolioRequest
            {
                Name = "Test Portfolio",
                InitialValue = 1000,
                Description = "Test Description"
            };

            _currentUserServiceMock.Setup(x => x.Id).Returns("testUserId");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _portfolioDbSetMock.Verify(x => x.Add(It.IsAny<Portfolio>()), Times.Once);
            _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDuplicateName_ReturnsFailure()
        {
            // Arrange
            var request = new CreatePortfolioRequest
            {
                Name = "Existing Portfolio",
                InitialValue = 1000,
                Description = "Test Description"
            };

            _currentUserServiceMock.Setup(x => x.Id).Returns("testUserId");
            _portfolios.Add(new Portfolio
            {
                Name = "Existing Portfolio",
                UserId = "testUserId",
                IsDelete = false
            });
            _portfolioDbSetMock.SetupData(_portfolios);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Portfolio with this name already exists", result.Errors);
        }

        [Fact]
        public async Task Handle_WithMissingRequiredFields_ReturnsFailure()
        {
            // Arrange
            var request = new CreatePortfolioRequest
            {
                Name = "",
                InitialValue = 0,
                Description = "Test Description"
            };

            _currentUserServiceMock.Setup(x => x.Id).Returns("testUserId");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Name is required", result.Errors);
        }

        [Fact]
        public async Task Handle_WithMaxLengthExceeded_ReturnsFailure()
        {
            // Arrange
            var request = new CreatePortfolioRequest
            {
                Name = new string('a', 101),
                InitialValue = 1000,
                Description = "Test Description"
            };

            _currentUserServiceMock.Setup(x => x.Id).Returns("testUserId");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Name cannot exceed 100 characters", result.Errors);
        }

        [Fact]
        public async Task Handle_WhenSaveFails_ReturnsFailure()
        {
            // Arrange
            var request = new CreatePortfolioRequest
            {
                Name = "Test Portfolio",
                InitialValue = 1000,
                Description = "Test Description"
            };

            _currentUserServiceMock.Setup(x => x.Id).Returns("testUserId");
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Save failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to create portfolio", result.Errors);
        }
    }
} 