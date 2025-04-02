using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Investment> Investments { get; }
    DbSet<Portfolio> Portfolios { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 