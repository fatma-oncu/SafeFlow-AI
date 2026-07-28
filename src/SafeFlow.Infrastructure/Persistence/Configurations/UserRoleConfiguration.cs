using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Identity.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the <see cref="UserRole"/> join entity.
/// </summary>
internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", "identity");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();

        // Composite unique: a user can only hold a role once
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
               .IsUnique()
               .HasDatabaseName("IX_UserRoles_UserId_RoleId");

        // ── Audit fields ─────────────────────────────────────────────────────
        builder.Property(ur => ur.CreatedAt).IsRequired();
        builder.Property(ur => ur.CreatedBy).HasMaxLength(100);
        builder.Property(ur => ur.LastModifiedAt);
        builder.Property(ur => ur.LastModifiedBy).HasMaxLength(100);
        builder.Property(ur => ur.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(ur => ur.DeletedAt);
        builder.Property(ur => ur.DeletedBy).HasMaxLength(100);
    }
}
