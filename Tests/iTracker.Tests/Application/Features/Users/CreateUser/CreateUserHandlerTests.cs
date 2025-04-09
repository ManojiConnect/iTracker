using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Application.Abstractions.Data;
using Application.Features.Users.CreateUser;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Application.Abstractions.Services;
using Moq;
using Xunit;

namespace iTracker.Tests.Application.Features.Users.CreateUser;

public class CreateUserHandlerTests
{
    private readonly Mock<UserManager<Infrastructure.Identity.ApplicationUser>> _userManager;
    private readonly Mock<IContext> _context;
    private readonly Mock<ILogger<CreateUserHandler>> _logger;
    private readonly Mock<IConfiguration> _configuration;
    private readonly Mock<IMailService> _mailService;
    private readonly Mock<RoleManager<IdentityRole>> _roleManager;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        var userStore = new Mock<IUserStore<Infrastructure.Identity.ApplicationUser>>();
        _userManager = new Mock<UserManager<Infrastructure.Identity.ApplicationUser>>(
            userStore.Object, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null);
        _context = new Mock<IContext>();
        _logger = new Mock<ILogger<CreateUserHandler>>();
        _configuration = new Mock<IConfiguration>();
        _mailService = new Mock<IMailService>();
        
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, 
            null, 
            null, 
            null, 
            null);

        _handler = new CreateUserHandler(
            _context.Object,
            _logger.Object,
            _configuration.Object,
            _userManager.Object,
            _mailService.Object,
            _roleManager.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            Role = "User",
            IsActive = true
        };

        var createdUser = new Infrastructure.Identity.ApplicationUser
        {
            Id = "testUserId",
            Email = request.Email,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            PhoneNumber = request.PhoneNumber,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow,
            EmailConfirmed = true,
            IsActive = true
        };

        // Setup FindByEmailAsync to return null first (user doesn't exist) and then the created user
        _userManager.SetupSequence(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((Infrastructure.Identity.ApplicationUser)null)
            .ReturnsAsync(createdUser);

        // Setup CreateAsync to return success
        _userManager.Setup(x => x.CreateAsync(It.Is<Infrastructure.Identity.ApplicationUser>(u => 
            u.Email == request.Email &&
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName &&
            u.PhoneNumber == request.PhoneNumber), 
            request.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Setup AddToRoleAsync to return success
        _userManager.Setup(x => x.AddToRoleAsync(It.Is<Infrastructure.Identity.ApplicationUser>(u => u.Email == request.Email), request.Role!))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _userManager.Verify(x => x.FindByEmailAsync(request.Email), Times.Exactly(2));
        _userManager.Verify(x => x.CreateAsync(It.Is<Infrastructure.Identity.ApplicationUser>(u => 
            u.Email == request.Email &&
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName &&
            u.PhoneNumber == request.PhoneNumber), 
            request.Password), Times.Once);
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<Infrastructure.Identity.ApplicationUser>(u => u.Email == request.Email), request.Role!), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFalse()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            Role = "User"
        };

        var existingUser = new Infrastructure.Identity.ApplicationUser
        {
            Email = request.Email,
            FirstName = "Existing",
            LastName = "User",
            PhoneNumber = "0987654321",
            CreatedBy = "System"
        };

        _userManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Errors.First());
    }
}