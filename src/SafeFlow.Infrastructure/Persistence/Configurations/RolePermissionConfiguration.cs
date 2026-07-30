using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Identity.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the <see cref="RolePermission"/> join entity.
/// </summary>
/// <remarks>
/// The <c>Permission</c> value object is mapped as an EF Core owned entity,
/// inlining <c>Module</c> and <c>Action</c> columns directly into this table.
/// Domain invariants (duplicate permission prevention) are enforced in <c>Role.AddPermission</c>.
/// </remarks>
internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "identity");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.RoleId).IsRequired();

        // ── Permission value object (owned) ───────────────────────────────────
        builder.OwnsOne(rp => rp.Permission, perm =>
        {
            perm.Property(p => p.Module)
                .HasColumnName("PermissionModule")
                .HasMaxLength(100)
                .IsRequired();

            perm.Property(p => p.Action)
                .HasColumnName("PermissionAction")
                .HasMaxLength(100)
                .IsRequired();
        });

        // ── Audit fields ─────────────────────────────────────────────────────
        builder.Property(rp => rp.CreatedAt).IsRequired();
        builder.Property(rp => rp.CreatedBy).HasMaxLength(100);
        builder.Property(rp => rp.LastModifiedAt);
        builder.Property(rp => rp.LastModifiedBy).HasMaxLength(100);
        builder.Property(rp => rp.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(rp => rp.DeletedAt);
        builder.Property(rp => rp.DeletedBy).HasMaxLength(100);
    }
}
