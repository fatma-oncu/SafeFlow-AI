using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RiskAssessmentApproval"/> audit entry entity.
/// </summary>
internal sealed class RiskAssessmentApprovalConfiguration : IEntityTypeConfiguration<RiskAssessmentApproval>
{
    public void Configure(EntityTypeBuilder<RiskAssessmentApproval> builder)
    {
        builder.ToTable("RiskAssessmentApprovals", "risk");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.RiskAssessmentId)
               .IsRequired();

        builder.Property(a => a.EmployeeId)
               .IsRequired();

        builder.Property(a => a.Decision)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(a => a.Comment)
               .HasMaxLength(1000);

        builder.Property(a => a.ProcessedAt)
               .IsRequired();
    }
}
