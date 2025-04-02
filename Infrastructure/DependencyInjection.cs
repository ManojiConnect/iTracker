using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Instead of registering another DbContext, we rely on what's registered by WebApp/Configurations/PersistanceSetup.cs
        
        // Register IUserManager
        services.AddScoped<IUserManager, ApplicationUserManager>();

        return services;
    }
} 