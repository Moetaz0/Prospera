using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class RealEstateEvaluationConfiguration : IEntityTypeConfiguration<RealEstateEvaluation>
{
    public void Configure(EntityTypeBuilder<RealEstateEvaluation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Location)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.CountryCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(r => r.PurchasePrice)
            .HasPrecision(18, 2);

        builder.Property(r => r.CurrentEstimatedValue)
            .HasPrecision(18, 2);

        builder.Property(r => r.AnnualGrowthRate)
            .HasPrecision(18, 2);

        builder.Property(r => r.TotalAppreciationPct)
            .HasPrecision(18, 2);

        builder.Property(r => r.Forecast5YearValue)
            .HasPrecision(18, 2);

        builder.Property(r => r.Forecast10YearValue)
            .HasPrecision(18, 2);

        builder.Property(r => r.EvaluatedAt)
            .IsRequired();

        builder.Property(r => r.Source)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.AssetId)
            .IsRequired();

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.AssetId);
    }
}
