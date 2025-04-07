using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Application.Common.Interfaces;
using Infrastructure.Identity;

namespace Infrastructure.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IContext
{
    private IDbContextTransaction? _currentTransaction;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<InvestmentCategory> InvestmentCategories { get; set; }
    public DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
    public DbSet<InvestmentHistory> InvestmentHistories { get; set; }
    public new DbSet<User> Users { get; set; }
    public DbSet<Lookup> Lookups { get; set; }
    public DbSet<LookupType> LookupTypes { get; set; }
    public DbSet<SystemSettings> SystemSettings { get; set; }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure decimal precision and scale
        builder.Entity<Investment>(entity =>
        {
            entity.Property(e => e.CurrentValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReturnPercentage).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TotalInvestment).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnrealizedGainLoss).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PurchaseDate).HasColumnType("datetime2");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        builder.Entity<InvestmentHistory>(entity =>
        {
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RecordedDate).HasColumnType("datetime2");
        });

        builder.Entity<InvestmentTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Units).HasColumnType("decimal(18,4)");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime2");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        builder.Entity<Portfolio>(entity =>
        {
            entity.Property(e => e.InitialValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReturnPercentage).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TotalInvestment).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnrealizedGainLoss).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        // Configure Identity columns to use provider-agnostic types
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(450)");
            entity.Property(e => e.UserName).HasColumnType("varchar(256)");
            entity.Property(e => e.NormalizedUserName).HasColumnType("varchar(256)");
            entity.Property(e => e.Email).HasColumnType("varchar(256)");
            entity.Property(e => e.NormalizedEmail).HasColumnType("varchar(256)");
            entity.Property(e => e.FirstName).HasColumnType("varchar(100)");
            entity.Property(e => e.LastName).HasColumnType("varchar(100)");
            entity.Property(e => e.CreatedBy).HasColumnType("varchar(450)");
            entity.Property(e => e.LastModifiedBy).HasColumnType("varchar(450)");
            entity.Property(e => e.Language).HasColumnType("varchar(10)");
            entity.Property(e => e.ProfileUrl).HasColumnType("varchar(1000)");
            entity.Property(e => e.PasswordHash).HasColumnType("varchar(1000)");
            entity.Property(e => e.SecurityStamp).HasColumnType("varchar(1000)");
            entity.Property(e => e.ConcurrencyStamp).HasColumnType("varchar(1000)");
            entity.Property(e => e.PhoneNumber).HasColumnType("varchar(20)");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime2");
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime2");
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(450)");
            entity.Property(e => e.Name).HasColumnType("varchar(256)");
            entity.Property(e => e.NormalizedName).HasColumnType("varchar(256)");
            entity.Property(e => e.ConcurrencyStamp).HasColumnType("varchar(1000)");
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(450)");
            entity.Property(e => e.RoleId).HasColumnType("varchar(450)");
        });

        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(450)");
            entity.Property(e => e.ClaimType).HasColumnType("varchar(1000)");
            entity.Property(e => e.ClaimValue).HasColumnType("varchar(1000)");
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(e => e.LoginProvider).HasColumnType("varchar(450)");
            entity.Property(e => e.ProviderKey).HasColumnType("varchar(450)");
            entity.Property(e => e.ProviderDisplayName).HasColumnType("varchar(1000)");
            entity.Property(e => e.UserId).HasColumnType("varchar(450)");
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(450)");
            entity.Property(e => e.LoginProvider).HasColumnType("varchar(450)");
            entity.Property(e => e.Name).HasColumnType("varchar(450)");
            entity.Property(e => e.Value).HasColumnType("varchar(2000)");
        });

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnType("varchar(450)");
            entity.Property(e => e.ClaimType).HasColumnType("varchar(1000)");
            entity.Property(e => e.ClaimValue).HasColumnType("varchar(1000)");
        });

        // Configure other entities
        builder.Entity<InvestmentCategory>(entity =>
        {
            entity.Property(e => e.Name).HasColumnType("varchar(100)");
            entity.Property(e => e.Description).HasColumnType("varchar(500)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        builder.Entity<LookupType>(entity =>
        {
            entity.Property(e => e.Name).HasColumnType("varchar(100)");
            entity.Property(e => e.Description).HasColumnType("varchar(500)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        builder.Entity<Portfolio>(entity =>
        {
            entity.Property(e => e.Name).HasColumnType("varchar(100)");
            entity.Property(e => e.Description).HasColumnType("varchar(500)");
            entity.Property(e => e.UserId).HasColumnType("varchar(450)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        builder.Entity<SystemSettings>(entity =>
        {
            entity.Property(e => e.CurrencySymbol).HasColumnType("varchar(10)");
            entity.Property(e => e.DecimalSeparator).HasColumnType("varchar(1)");
            entity.Property(e => e.ThousandsSeparator).HasColumnType("varchar(1)");
            entity.Property(e => e.DateFormat).HasColumnType("varchar(20)");
            entity.Property(e => e.SettingKey).HasColumnType("varchar(100)");
            entity.Property(e => e.SettingValue).HasColumnType("varchar(1000)");
            entity.Property(e => e.PerformanceCalculationMethod).HasColumnType("varchar(50)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime2");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime2");
        });

        // Configure table names for Identity tables
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        builder.Entity<IdentityRole>().ToTable("AspNetRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");

        // Seed investment categories
        var now = DateTime.UtcNow;

        // Configure seed data for InvestmentCategory
        var investmentCategories = new[]
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

        // Configure seed data for ApplicationUser
        var adminUser = new ApplicationUser
        {
            Id = "1",
            UserName = "admin@itracker.com",
            NormalizedUserName = "ADMIN@ITRACKER.COM",
            Email = "admin@itracker.com",
            NormalizedEmail = "ADMIN@ITRACKER.COM",
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

        // Configure seed data for IdentityRole
        var roles = new[]
        {
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        };

        // Apply seed data
        builder.Entity<InvestmentCategory>().HasData(investmentCategories);
        builder.Entity<ApplicationUser>().HasData(adminUser);
        builder.Entity<IdentityRole>().HasData(roles);
    }
}
