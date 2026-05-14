using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class CarEvaluationConfiguration : IEntityTypeConfiguration<CarEvaluation>
{
    public void Configure(EntityTypeBuilder<CarEvaluation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Make)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.CountryCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.PurchasePrice)
            .HasPrecision(18, 2);

        builder.Property(c => c.CurrentMarketValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.TotalDepreciationPct)
            .HasPrecision(18, 2);

        builder.Property(c => c.AnnualDepreciationRate)
            .HasPrecision(18, 2);

        builder.Property(c => c.InflationAdjustedValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.Forecast5YearValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.EvaluatedAt)
            .IsRequired();

        builder.Property(c => c.Source)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.AssetId)
            .IsRequired();

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.AssetId);
    }
}
