using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.Data;

public interface IContext
{
    DatabaseFacade Database { get; }
    DbSet<Portfolio> Portfolios { get; set; }
    DbSet<Investment> Investments { get; set; }
    DbSet<InvestmentCategory> InvestmentCategories { get; set; }
    DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
    DbSet<InvestmentHistory> InvestmentHistories { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<Lookup> Lookups { get; set; }
    DbSet<LookupType> LookupTypes { get; set; }
    DbSet<SystemSettings> SystemSettings { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
} 