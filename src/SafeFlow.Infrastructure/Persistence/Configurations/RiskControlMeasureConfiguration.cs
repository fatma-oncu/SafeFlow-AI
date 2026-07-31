using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RiskControlMeasure"/> child entity.
/// </summary>
internal sealed class RiskControlMeasureConfiguration : IEntityTypeConfiguration<RiskControlMeasure>
{
    public void Configure(EntityTypeBuilder<RiskControlMeasure> builder)
    {
        builder.ToTable("RiskControlMeasures", "risk");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.RiskHazardId)
               .IsRequired();

        // ── Description value object (owned) ─────────────────────────────────
        builder.OwnsOne(c => c.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.Property(c => c.Type)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.IsImplemented)
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(c => c.ImplementedAt);
    }
}
