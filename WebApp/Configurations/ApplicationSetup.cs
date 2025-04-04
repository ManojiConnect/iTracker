using Application.Common.Interfaces;
using Application.Services;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApp.Configurations;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<Application.Abstractions.Interfaces.ISettingsService, SettingsService>();
        services.AddScoped<OtpService>();
        services.AddScoped<IMailService>(sp => 
            new SendGridService(
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