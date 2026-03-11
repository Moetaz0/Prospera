using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class InvestmentRecommendationConfiguration : IEntityTypeConfiguration<InvestmentRecommendation>
{
    public void Configure(EntityTypeBuilder<InvestmentRecommendation> builder)
    {
        builder.HasKey(ir => ir.Id);

        builder.Property(ir => ir.UserId)
            .IsRequired();

        builder.Property(ir => ir.SuggestedAllocation)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(ir => ir.Explanation)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(ir => ir.CreatedAt)
            .IsRequired();

        builder.HasIndex(ir => ir.UserId);
        builder.HasIndex(ir => ir.CreatedAt);
    }
}
