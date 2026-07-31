using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.RiskAssessments.Aggregates;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RiskAssessment"/> aggregate root.
/// </summary>
internal sealed class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ToTable("RiskAssessments", "risk");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        // ── AssessmentNumber value object (owned) ───────────────────────────
        builder.OwnsOne(r => r.AssessmentNumber, num =>
        {
            num.Property(n => n.Value)
               .HasColumnName("AssessmentNumber")
               .HasMaxLength(50)
               .IsRequired();

            num.HasIndex(n => n.Value)
               .IsUnique()
               .HasDatabaseName("IX_RiskAssessments_AssessmentNumber");
        });

        builder.Property(r => r.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(r => r.Description)
               .HasMaxLength(2000);

        builder.Property(r => r.DepartmentId)
               .IsRequired();

        builder.HasIndex(r => r.DepartmentId)
               .HasDatabaseName("IX_RiskAssessments_DepartmentId");

        builder.Property(r => r.CreatedByEmployeeId)
               .IsRequired();

        builder.Property(r => r.ResponsibleEmployeeId)
               .IsRequired();

        builder.HasIndex(r => r.ResponsibleEmployeeId)
               .HasDatabaseName("IX_RiskAssessments_ResponsibleEmployeeId");

        builder.Property(r => r.ApprovedByEmployeeId);

        builder.Property(r => r.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.HasIndex(r => r.Status)
               .HasDatabaseName("IX_RiskAssessments_Status");

        builder.Property(r => r.OverallRiskLevel)
               .HasConversion<int>()
               .IsRequired();

        builder.HasIndex(r => r.OverallRiskLevel)
               .HasDatabaseName("IX_RiskAssessments_OverallRiskLevel");

        builder.Property(r => r.RevisionNumber)
               .HasDefaultValue(1)
               .IsRequired();

        builder.Property(r => r.PreviousAssessmentId);

        builder.Property(r => r.NextReviewDate);

        builder.Property(r => r.TenantId)
               .IsRequired();

        // ── Optimistic Concurrency RowVersion ─────────────────────────────────
        builder.Property(r => r.RowVersion);

        // ── Audit & Soft Delete Fields ────────────────────────────────────────
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedAt);
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);
        builder.Property(r => r.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(r => r.DeletedAt);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        // ── Global query filter for soft delete ──────────────────────────────
        builder.HasQueryFilter(r => !r.IsDeleted);

        // ── Navigation: Hazards & Approvals ──────────────────────────────────
        builder.HasMany(r => r.Hazards)
               .WithOne()
               .HasForeignKey(h => h.RiskAssessmentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Approvals)
               .WithOne()
               .HasForeignKey(a => a.RiskAssessmentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.History)
               .WithOne()
               .HasForeignKey(h => h.RiskAssessmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
