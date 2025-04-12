using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Configurations;
using WebApp.Models;
using WebApp.Services;
using Xunit;

namespace Tests.WebApp.Configurations;

public class CookieAuthenticationOptionsSetupTests
{
    [Fact]
    public void Configure_WithCustomSessionTimeout_SetsCorrectExpiration()
    {
        // Arrange
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 45 });

        var optionsSetup = new CookieAuthenticationOptionsSetup(settingsService.Object);
        var options = new CookieAuthenticationOptions();

        // Act
        optionsSetup.Configure(options);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(45), options.ExpireTimeSpan);
    }

    [Fact]
    public void Configure_WithDefaultSessionTimeout_SetsDefaultExpiration()
    {
        // Arrange
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 30 }); // Default value

        var optionsSetup = new CookieAuthenticationOptionsSetup(settingsService.Object);
        var options = new CookieAuthenticationOptions();

        // Act
        optionsSetup.Configure(options);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(30), options.ExpireTimeSpan);
    }

    [Fact]
    public void Configure_WithInvalidSessionTimeout_ThrowsException()
    {
        // Arrange
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 0 }); // Invalid value

        var optionsSetup = new CookieAuthenticationOptionsSetup(settingsService.Object);
        var options = new CookieAuthenticationOptions();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => optionsSetup.Configure(options));
        Assert.Contains("Invalid session timeout value", exception.Message);
    }
    
    [Fact]
    public void Configure_WithTooLargeSessionTimeout_ThrowsException()
    {
        // Arrange
        var settingsService = new Mock<IApplicationSettingsService>();
        
        settingsService.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsViewModel { SessionTimeoutMinutes = 200 }); // Too large

        var optionsSetup = new CookieAuthenticationOptionsSetup(settingsService.Object);
        var options = new CookieAuthenticationOptions();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => optionsSetup.Configure(options));
        Assert.Contains("Invalid session timeout value", exception.Message);
    }
} 