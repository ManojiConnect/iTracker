using Application.Common.Interfaces;
using Application.Abstractions.Services;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Application.Services;

namespace WebApp.Configurations;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        
        // Register services
        services.AddScoped<Application.Abstractions.Services.ICurrentUserService, Infrastructure.Common.CurrentUserService>();
        services.AddScoped<Application.Services.ILookupService, Application.Services.LookupService>();
        services.AddScoped<Application.Abstractions.Services.ISettingsService, Infrastructure.Services.SettingsService>();
        services.AddScoped<Application.Services.OtpService>();
        services.AddScoped<Application.Abstractions.Services.IMailService>(sp => 
            new Application.Services.SendGridService(
                configuration.GetValue<string>("SendGridSettings:ApiKey") ?? throw new InvalidOperationException("SendGrid API key is not configured"),
                configuration
            ));

        // Configure Identity
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 1;
            options.Password.RequiredUniqueChars = 0;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }
}