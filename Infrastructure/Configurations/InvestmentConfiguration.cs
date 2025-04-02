using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.TotalInvestment)
            .HasPrecision(18, 2);

        builder.Property(i => i.CurrentValue)
            .HasPrecision(18, 2);

        builder.Property(i => i.UnrealizedGainLoss)
            .HasPrecision(18, 2);

        builder.Property(i => i.ReturnPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Investments)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Portfolio)
            .WithMany(p => p.Investments)
            .HasForeignKey(i => i.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 