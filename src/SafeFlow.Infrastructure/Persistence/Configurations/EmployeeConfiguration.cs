using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Employees.Aggregates;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Employee"/> aggregate root.
/// </summary>
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "employee");

        builder.HasKey(e => e.Id);

        // ── EmployeeNumber value object (owned) ─────────────────────────────
        builder.OwnsOne(e => e.EmployeeNumber, num =>
        {
            num.Property(n => n.Value)
               .HasColumnName("EmployeeNumber")
               .HasMaxLength(50)
               .IsRequired();

            num.HasIndex(n => n.Value)
               .IsUnique()
               .HasDatabaseName("IX_Employees_EmployeeNumber");
        });

        // ── Email value object (owned) ────────────────────────────────────────
        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(m => m.Value)
                 .HasColumnName("Email")
                 .HasMaxLength(254)
                 .IsRequired();

            email.HasIndex(m => m.Value)
                 .HasDatabaseName("IX_Employees_Email");
        });

        // ── PhoneNumber value object (owned optional) ───────────────────────
        builder.OwnsOne(e => e.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                 .HasColumnName("PhoneNumber")
                 .HasMaxLength(20);
        });

        // ── DepartmentId value object (owned) ────────────────────────────────
        builder.OwnsOne(e => e.DepartmentId, dept =>
        {
            dept.Property(d => d.Value)
                .HasColumnName("DepartmentId")
                .IsRequired();

            dept.HasIndex(d => d.Value)
                .HasDatabaseName("IX_Employees_DepartmentId");
        });

        // ── JobTitle value object (owned) ────────────────────────────────────
        builder.OwnsOne(e => e.JobTitle, title =>
        {
            title.Property(t => t.Value)
                 .HasColumnName("JobTitle")
                 .HasMaxLength(100)
                 .IsRequired();
        });

        builder.Property(e => e.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(e => e.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(e => e.EmploymentType)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(e => e.EmploymentStatus)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(e => e.HireDate)
               .IsRequired();

        builder.Property(e => e.UserId);

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(e => e.TenantId)
               .IsRequired();

        // ── Optimistic Concurrency RowVersion ─────────────────────────────────
        builder.Property(e => e.RowVersion)
               .IsConcurrencyToken();

        // ── Audit & Soft Delete Fields ────────────────────────────────────────
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.LastModifiedAt);
        builder.Property(e => e.LastModifiedBy).HasMaxLength(100);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.DeletedBy).HasMaxLength(100);

        // ── Global query filter for soft delete ──────────────────────────────
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
