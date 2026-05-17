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

        builder.Property(ir => ir.AnalysisContext)
            .IsRequired(false)
            .HasMaxLength(2048);

        builder.Property(ir => ir.CreatedAt)
            .IsRequired();

        builder.Property(ir => ir.SessionId)
            .IsRequired(false);

        // Structured recommendation fields - optional for backward compatibility with existing MongoDB documents
        builder.Property(ir => ir.RiskProfile)
            .IsRequired(false);
        builder.Property(ir => ir.PortfolioSummary)
            .IsRequired(false);
        builder.Property(ir => ir.SuggestedActions)
            .IsRequired(false);
        builder.Property(ir => ir.KeyRisks)
            .IsRequired(false);
        builder.Property(ir => ir.Opportunities)
            .IsRequired(false);

        // Configure AllocationItems as owned collection stored as JSON
        builder.OwnsMany(ir => ir.AllocationItems, a =>
        {
            a.ToJson();
        });

        builder.HasIndex(ir => ir.UserId);
        builder.HasIndex(ir => ir.SessionId);
        builder.HasIndex(ir => ir.CreatedAt);
    }
}
