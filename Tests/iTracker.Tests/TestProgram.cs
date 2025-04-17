using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Abstractions.Services;
using Infrastructure.Services;

namespace iTracker.Tests;

public class TestProgram
{
    public static Microsoft.AspNetCore.Builder.WebApplication CreateTestApplication()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();

        // Add services to the container
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        // Add other required services
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ISettingsService, SettingsService>();

        var app = builder.Build();

        return app;
    }
} 