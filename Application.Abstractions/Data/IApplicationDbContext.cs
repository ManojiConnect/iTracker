using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; set; }
    DbSet<Investment> Investments { get; set; }
    DbSet<Portfolio> Portfolios { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 