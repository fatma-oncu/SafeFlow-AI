using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Identity.Aggregates;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the <see cref="Role"/> aggregate root.
/// </summary>
internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "identity");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasIndex(r => r.Name)
               .IsUnique()
               .HasDatabaseName("IX_Roles_Name");

        builder.Property(r => r.Description)
               .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
               .HasDefaultValue(false)
               .IsRequired();

        // ── Audit fields ─────────────────────────────────────────────────────
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedAt);
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);
        builder.Property(r => r.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(r => r.DeletedAt);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        builder.HasQueryFilter(r => !r.IsDeleted);

        // ── Navigation: RolePermissions ───────────────────────────────────────
        builder.HasMany(r => r.RolePermissions)
               .WithOne()
               .HasForeignKey(rp => rp.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
