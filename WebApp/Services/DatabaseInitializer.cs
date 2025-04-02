using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Services;

public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            await CreateRolesAsync(roleManager);
            await CreateAdminUserAsync(userManager);
            
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    private async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "User" };

        foreach (var roleName in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("Created role {RoleName}", roleName);
            }
        }
    }

    private async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "Admin@itrackerApp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var result = await userManager.CreateAsync(adminUser, "Test@123");
            if (result.Succeeded)
            {
                _logger.LogInformation("Created admin user with email {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create admin user: {Errors}", errors);
                return;
            }
        }

        // Ensure admin user is in Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation("Added admin user to Admin role");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to add admin to role: {Errors}", errors);
            }
        }
    }
} 