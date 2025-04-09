using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApp.Configurations;
using WebApp.Services;
using Xunit;

namespace Tests.WebApp.Configurations;

public class AuthSetupTests
{
    [Fact]
    public void AddAuthSetup_WithCustomSessionTimeout_SetsCorrectExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 45 });

        services.AddScoped<IApplicationSettingsService>(_ => settingsService.Object);

        // Act
        services.AddAuthSetup(configuration.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CookieAuthenticationOptions>>();
        
        Assert.Equal(TimeSpan.FromMinutes(45), options.Value.ExpireTimeSpan);
    }

    [Fact]
    public void AddAuthSetup_WithDefaultSessionTimeout_SetsDefaultExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 30 }); // Default value

        services.AddScoped<IApplicationSettingsService>(_ => settingsService.Object);

        // Act
        services.AddAuthSetup(configuration.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CookieAuthenticationOptions>>();
        
        Assert.Equal(TimeSpan.FromMinutes(30), options.Value.ExpireTimeSpan);
    }

    [Fact]
    public void AddAuthSetup_WithInvalidSessionTimeout_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 0 }); // Invalid value

        services.AddScoped<IApplicationSettingsService>(_ => settingsService.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddAuthSetup(configuration.Object));
    }
} 