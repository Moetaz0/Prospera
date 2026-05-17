using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class RecommendationSessionConfiguration : IEntityTypeConfiguration<RecommendationSession>
{
    public void Configure(EntityTypeBuilder<RecommendationSession> builder)
    {
        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.UserId)
            .IsRequired();

        builder.Property(rs => rs.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(rs => rs.Description)
            .HasMaxLength(1024);

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt)
            .IsRequired();

        // Configure the relationship without shadow properties
        // MongoDB doesn't support foreign key constraints, so we just keep SessionId as a regular property
        builder.Ignore(rs => rs.Recommendations);

        builder.HasIndex(rs => rs.UserId);
    }
}
