using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RiskHazard"/> child entity.
/// </summary>
internal sealed class RiskHazardConfiguration : IEntityTypeConfiguration<RiskHazard>
{
    public void Configure(EntityTypeBuilder<RiskHazard> builder)
    {
        builder.ToTable("RiskHazards", "risk");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.RiskAssessmentId)
               .IsRequired();

        // ── Description value object (owned) ─────────────────────────────────
        builder.OwnsOne(h => h.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(500)
                .IsRequired();
        });

        // ── InitialScore value object (owned) ────────────────────────────────
        builder.OwnsOne(h => h.InitialScore, score =>
        {
            score.Property(s => s.Likelihood)
                 .HasColumnName("InitialLikelihood")
                 .HasConversion<int>()
                 .IsRequired();

            score.Property(s => s.Severity)
                 .HasColumnName("InitialSeverity")
                 .HasConversion<int>()
                 .IsRequired();

            score.Property(s => s.Score)
                 .HasColumnName("InitialScore")
                 .IsRequired();

            score.Property(s => s.RiskLevel)
                 .HasColumnName("InitialRiskLevel")
                 .HasConversion<int>()
                 .IsRequired();
        });

        // ── ResidualScore value object (owned) ───────────────────────────────
        builder.OwnsOne(h => h.ResidualScore, score =>
        {
            score.Property(s => s.Likelihood)
                 .HasColumnName("ResidualLikelihood")
                 .HasConversion<int>()
                 .IsRequired();

            score.Property(s => s.Severity)
                 .HasColumnName("ResidualSeverity")
                 .HasConversion<int>()
                 .IsRequired();

            score.Property(s => s.Score)
                 .HasColumnName("ResidualScore")
                 .IsRequired();

            score.Property(s => s.RiskLevel)
                 .HasColumnName("ResidualRiskLevel")
                 .HasConversion<int>()
                 .IsRequired();
        });

        // ── Navigation: ControlMeasures ─────────────────────────────────────
        builder.HasMany(h => h.ControlMeasures)
               .WithOne()
               .HasForeignKey(c => c.RiskHazardId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
