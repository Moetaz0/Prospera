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

        // Structured recommendation fields - optional for backward compatibility
        builder.Property(ir => ir.RiskProfile);
        builder.Property(ir => ir.PortfolioSummary);
        builder.Property(ir => ir.SuggestedActions);
        builder.Property(ir => ir.KeyRisks);
        builder.Property(ir => ir.Opportunities);

        // Configure AllocationItems as an owned collection of embedded documents
        builder.OwnsMany(ir => ir.AllocationItems, a =>
        {
            a.ToJson();
            a.Property(item => item.Category);
            a.Property(item => item.Percentage);
            a.Property(item => item.Rationale);
        });

        builder.HasIndex(ir => ir.UserId);
        builder.HasIndex(ir => ir.CreatedAt);
    }
}
