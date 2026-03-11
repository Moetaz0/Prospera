using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.CurrentValue)
            .HasPrecision(18, 2);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasIndex(a => a.UserId);
    }
}
