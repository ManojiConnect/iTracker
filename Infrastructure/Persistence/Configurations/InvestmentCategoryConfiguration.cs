using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InvestmentCategoryConfiguration : IEntityTypeConfiguration<InvestmentCategory>
{
    public void Configure(EntityTypeBuilder<InvestmentCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Seed default categories
        builder.HasData(
            new InvestmentCategory
            {
                Id = 1,
                Name = "Stocks",
                Description = "Individual company stocks",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            },
            new InvestmentCategory
            {
                Id = 2,
                Name = "Mutual Funds",
                Description = "Pooled investment funds",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            },
            new InvestmentCategory
            {
                Id = 3,
                Name = "ETFs",
                Description = "Exchange-traded funds",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            },
            new InvestmentCategory
            {
                Id = 4,
                Name = "Bonds",
                Description = "Fixed income securities",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            },
            new InvestmentCategory
            {
                Id = 5,
                Name = "Real Estate",
                Description = "Real estate investments",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            },
            new InvestmentCategory
            {
                Id = 6,
                Name = "Cryptocurrency",
                Description = "Digital currencies and tokens",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1
            }
        );
    }
} 