using System;
using System.Threading.Tasks;
using Infrastructure.Context;
using Infrastructure.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var now = DateTime.UtcNow;

            // Seed roles if they don't exist
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    _logger.LogInformation("Creating role {Role}", role);
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user if it doesn't exist
            var adminEmail = "admin@itracker.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                _logger.LogInformation("Creating admin user");
                adminUser = new ApplicationUser
                {
                    Id = "1",
                    UserName = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper(),
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedDate = now,
                    LastModifiedDate = now,
                    IsActive = true,
                    CreatedBy = "System",
                    Language = "en",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created successfully");
                }
                else
                {
                    _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Seed investment categories if they don't exist
            if (!await context.InvestmentCategories.AnyAsync())
            {
                _logger.LogInformation("Seeding investment categories");
                var categories = new[]
                {
                    new InvestmentCategory 
                    { 
                        Id = 1, 
                        Name = "Stocks", 
                        Description = "Equity investments in publicly traded companies",
                        CreatedBy = 1,
                        CreatedOn = now,
                        IsActive = true,
                        IsDelete = false
                    },
                    new InvestmentCategory 
                    { 
                        Id = 2, 
                        Name = "Bonds", 
                        Description = "Fixed-income securities",
                        CreatedBy = 1,
                        CreatedOn = now,
                        IsActive = true,
                        IsDelete = false
                    },
                    new InvestmentCategory 
                    { 
                        Id = 3, 
                        Name = "Mutual Funds", 
                        Description = "Pooled investment vehicles",
                        CreatedBy = 1,
                        CreatedOn = now,
                        IsActive = true,
                        IsDelete = false
                    }
                };

                await context.InvestmentCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                _logger.LogInformation("Investment categories seeded successfully");
            }

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
} 