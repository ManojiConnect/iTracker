using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace WebApp.Configurations;

public static class PersistanceSetup
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Configure the DbContext
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            // Always use SQL Server in production
            if (environment.IsDevelopment() && !environment.IsProduction())
            {
                options.UseSqlite(
                    "Data Source=app.db",
                    x =>
                    {
                        x.MigrationsHistoryTable("__EFMigrationsHistory");
                        x.MigrationsAssembly("WebApp");
                        x.CommandTimeout(60);
                    });
            }
            else
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    x =>
                    {
                        x.MigrationsHistoryTable("__EFMigrationsHistory");
                        x.MigrationsAssembly("WebApp");
                        x.CommandTimeout(60);
                    });
            }
        });

        services.AddScoped<IContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}