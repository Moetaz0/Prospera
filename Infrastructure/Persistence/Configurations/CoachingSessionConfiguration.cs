using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Persistence.Configurations;

public class CoachingSessionConfiguration : IEntityTypeConfiguration<CoachingSession>
{
    public void Configure(EntityTypeBuilder<CoachingSession> builder)
    {
        builder.HasKey(cs => cs.Id);

        // Required for MongoDB provider to correctly track items in collections
        // This resolves the "shadow key property CoachingActionItem._unique is unknown" error
        builder.OwnsMany(cs => cs.ActionItems, a => 
        {
            a.HasKey(x => x.Id);
        });

        builder.OwnsMany(cs => cs.Milestones, m => 
        {
            m.HasKey(x => x.Id);
        });
        
        builder.HasIndex(cs => cs.UserId);
        builder.HasIndex(cs => cs.IsActive);
    }
}
