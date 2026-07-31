using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RiskAssessmentHistory"/> audit log entity.
/// </summary>
internal sealed class RiskAssessmentHistoryConfiguration : IEntityTypeConfiguration<RiskAssessmentHistory>
{
    public void Configure(EntityTypeBuilder<RiskAssessmentHistory> builder)
    {
        builder.ToTable("RiskAssessmentHistory", "risk");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.RiskAssessmentId)
               .IsRequired();

        builder.Property(h => h.Action)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(h => h.PerformedByEmployeeId)
               .IsRequired();

        builder.Property(h => h.OldStatus)
               .HasConversion<int>();

        builder.Property(h => h.NewStatus)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(h => h.Comment)
               .HasMaxLength(1000);

        builder.Property(h => h.CreatedAt)
               .IsRequired();
    }
}
