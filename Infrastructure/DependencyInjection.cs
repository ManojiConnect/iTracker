using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Instead of registering another DbContext, we rely on what's registered by WebApp/Configurations/PersistanceSetup.cs
        
        // Register IUserManager
        services.AddScoped<IUserManager, ApplicationUserManager>();
        
        // Register SettingsService using fully qualified name
        services.AddScoped<Application.Abstractions.Interfaces.ISettingsService, SettingsService>();
        
        // Register memory cache if not already registered
        services.AddMemoryCache();

        return services;
    }
} 