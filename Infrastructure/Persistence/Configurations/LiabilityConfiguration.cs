using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class LiabilityConfiguration : IEntityTypeConfiguration<Liability>
{
    public void Configure(EntityTypeBuilder<Liability> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Amount)
            .HasPrecision(18, 2);

        builder.Property(l => l.Type)
            .IsRequired();

        builder.Property(l => l.UserId)
            .IsRequired();

        builder.HasIndex(l => l.UserId);
    }
}
