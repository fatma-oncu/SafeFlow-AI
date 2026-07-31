using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Incidents.Aggregates;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity configuration for the <see cref="Incident"/> Aggregate Root.
/// Mapped under schema 'incident'.
/// </summary>
internal sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents", "incident");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        // ── Owned Value Objects ──────────────────────────────────────────────
        builder.OwnsOne(i => i.IncidentNumber, num =>
        {
            num.Property(n => n.Value)
               .HasColumnName("IncidentNumber")
               .HasMaxLength(20)
               .IsRequired();

            num.HasIndex(n => n.Value)
               .IsUnique()
               .HasDatabaseName("IX_Incidents_IncidentNumber");
        });

        builder.OwnsOne(i => i.Title, t =>
        {
            t.Property(x => x.Value)
             .HasColumnName("Title")
             .HasMaxLength(200)
             .IsRequired();
        });

        builder.OwnsOne(i => i.Description, d =>
        {
            d.Property(x => x.Value)
             .HasColumnName("Description")
             .HasMaxLength(4000)
             .IsRequired();
        });

        builder.OwnsOne(i => i.Location, l =>
        {
            l.Property(x => x.Value)
             .HasColumnName("Location")
             .HasMaxLength(500)
             .IsRequired();
        });

        // ── Enums & Scalar Properties ───────────────────────────────────────
        builder.Property(i => i.OccurredAt)
               .IsRequired();

        builder.Property(i => i.Severity)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(i => i.Category)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(i => i.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(i => i.InvestigationResult)
               .HasConversion<int>();

        builder.Property(i => i.ResolutionSummary)
               .HasMaxLength(2000);

        builder.Property(i => i.ClosureNotes)
               .HasMaxLength(1000);

        // ── Foreign Key Identifiers ───────────────────────────────────────────
        builder.Property(i => i.DepartmentId).IsRequired();
        builder.Property(i => i.ReportedByEmployeeId).IsRequired();
        builder.Property(i => i.AffectedEmployeeId);
        builder.Property(i => i.AssignedToEmployeeId);
        builder.Property(i => i.InvestigatedByEmployeeId);
        builder.Property(i => i.ClosedByEmployeeId);
        builder.Property(i => i.RiskAssessmentId);
        builder.Property(i => i.TenantId).IsRequired();

        // ── RowVersion Concurrency ───────────────────────────────────────────
        builder.Property(i => i.RowVersion);

        // ── Audit & Soft Delete Fields ────────────────────────────────────────
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(100);
        builder.Property(i => i.LastModifiedAt);
        builder.Property(i => i.LastModifiedBy).HasMaxLength(100);
        builder.Property(i => i.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(i => i.DeletedAt);
        builder.Property(i => i.DeletedBy).HasMaxLength(100);

        // ── Global Soft Delete Filter ─────────────────────────────────────────
        builder.HasQueryFilter(i => !i.IsDeleted);

        // ── Database Indexes ──────────────────────────────────────────────────
        builder.HasIndex(i => i.Status).HasDatabaseName("IX_Incidents_Status");
        builder.HasIndex(i => i.Severity).HasDatabaseName("IX_Incidents_Severity");
        builder.HasIndex(i => i.AssignedToEmployeeId).HasDatabaseName("IX_Incidents_AssignedToEmployeeId");
        builder.HasIndex(i => i.ReportedByEmployeeId).HasDatabaseName("IX_Incidents_ReportedByEmployeeId");
        builder.HasIndex(i => i.OccurredAt).HasDatabaseName("IX_Incidents_OccurredAt");

        // ── Child Navigations ─────────────────────────────────────────────────
        builder.HasMany(i => i.Attachments)
               .WithOne()
               .HasForeignKey(a => a.IncidentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Comments)
               .WithOne()
               .HasForeignKey(c => c.IncidentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.CorrectiveActions)
               .WithOne()
               .HasForeignKey(a => a.IncidentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
