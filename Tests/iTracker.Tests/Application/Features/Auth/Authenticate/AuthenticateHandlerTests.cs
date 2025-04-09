using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Abstractions.Data;
using Application.Features.Auth.Authenticate;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Ardalis.Result;

namespace iTracker.Tests.Application.Features.Auth.Authenticate;

public class AuthenticateHandlerTests
{
    private readonly Mock<ILogger<AuthenticateHandler>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<UserManager<Infrastructure.Identity.ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<Infrastructure.Identity.ApplicationUser>> _mockSignInManager;
    private readonly Mock<IContext> _mockContext;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IMediator> _mockMediator;
    private readonly AuthenticateHandler _handler;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IAuthenticationService> _authenticationService;
    private readonly DefaultHttpContext _httpContext;

    public AuthenticateHandlerTests()
    {
        _mockLogger = new Mock<ILogger<AuthenticateHandler>>();
        _mockConfiguration = new Mock<IConfiguration>();

        var userStore = new Mock<IUserStore<Infrastructure.Identity.ApplicationUser>>();
        var userValidators = new List<IUserValidator<Infrastructure.Identity.ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<Infrastructure.Identity.ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<Infrastructure.Identity.ApplicationUser>>>();

        _mockUserManager = new Mock<UserManager<Infrastructure.Identity.ApplicationUser>>(
            userStore.Object,
            null,
            null,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);

        _mockSignInManager = new Mock<SignInManager<Infrastructure.Identity.ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<Infrastructure.Identity.ApplicationUser>>(),
            null, null, null, null);

        _mockContext = new Mock<IContext>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockMediator = new Mock<IMediator>();

        _serviceProvider = new Mock<IServiceProvider>();
        _authenticationService = new Mock<IAuthenticationService>();

        _serviceProvider
            .Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(_authenticationService.Object);

        _httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider.Object
        };

        // Set up response headers for cookie authentication
        _httpContext.Response.Headers["Set-Cookie"] = "AuthCookie=value; path=/; HttpOnly; SameSite=Lax";

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        _handler = new AuthenticateHandler(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockContext.Object,
            _mockHttpContextAccessor.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var request = new AuthenticateRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = true
        };

        var user = new Infrastructure.Identity.ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = "Test",
            LastName = "User",
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(user, request.Password, request.RememberMe, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _authenticationService
            .Setup(x => x.SignInAsync(
                _httpContext,
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFalse()
    {
        // Arrange
        var request = new AuthenticateRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!",
            RememberMe = false
        };

        var user = new Infrastructure.Identity.ApplicationUser
        {
            Id = "testUserId",
            Email = request.Email,
            UserName = request.Email,
            FirstName = "Test",
            LastName = "User",
            CreatedBy = "system",
            IsActive = true
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, request.Password, request.RememberMe, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var request = new AuthenticateRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((Infrastructure.Identity.ApplicationUser)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var request = new AuthenticateRequest
        {
            Email = "inactive@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        var user = new Infrastructure.Identity.ApplicationUser
        {
            Id = "testUserId",
            Email = request.Email,
            UserName = request.Email,
            FirstName = "Test",
            LastName = "User",
            CreatedBy = "system",
            IsActive = false
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("User account is inactive.");
    }
}