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
        
        // Seed investment categories
        builder.Entity<InvestmentCategory>().HasData(
            new InvestmentCategory 
            { 
                Id = 1, 
                Name = "Stocks", 
                Description = "Equity investments in publicly traded companies",
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            },
            new InvestmentCategory 
            { 
                Id = 2, 
                Name = "Bonds", 
                Description = "Fixed income securities",
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            },
            new InvestmentCategory 
            { 
                Id = 3, 
                Name = "Real Estate", 
                Description = "Property and REITs",
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            },
            new InvestmentCategory 
            { 
                Id = 4, 
                Name = "Cryptocurrency", 
                Description = "Digital assets and tokens",
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            },
            new InvestmentCategory 
            { 
                Id = 5, 
                Name = "Mutual Funds", 
                Description = "Managed investment pools",
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            }
        );
    }
}
