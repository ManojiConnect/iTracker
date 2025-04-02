using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Configurations;

public static class UserSeeder
{
    public static void SeedUsers(this ModelBuilder builder)
    {
        // Seed admin user
        var adminUser = new ApplicationUser
        {
            Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
            UserName = "admin@localhost",
            NormalizedUserName = "ADMIN@LOCALHOST",
            Email = "admin@localhost",
            NormalizedEmail = "ADMIN@LOCALHOST",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAEAACcQAAAAEIE3TwP4EGZk7HHfVQl4nCXbZnW2y6h7Zn7oK0vM7amGS9o/N4iE2QQqfDWP4s+ZqQ==",
            SecurityStamp = "3LVBWZ7FAS74ZJNNYIUFGDVRU5NRPYQU",
            ConcurrencyStamp = "7c89df80-7e07-4251-9640-09be9bee25d5",
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            FirstName = "Admin",
            LastName = "User",
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        builder.Entity<ApplicationUser>().HasData(adminUser);

        // Seed roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
            new IdentityRole { Id = "e1823908-c7c9-4e53-980e-972fd4799f59", Name = "User", NormalizedName = "USER" }
        );

        // Seed user roles
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9", RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210" }
        );
    }
} 