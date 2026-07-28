using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the <see cref="User"/> aggregate root.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id);

        // ── Email value object (owned) ────────────────────────────────────────
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("Email")
                 .HasMaxLength(254)
                 .IsRequired();

            email.HasIndex(e => e.Value)
                 .IsUnique()
                 .HasDatabaseName("IX_Users_Email");
        });

        // ── FullName value object (owned) ─────────────────────────────────────
        builder.OwnsOne(u => u.FullName, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // ── PhoneNumber value object (optional owned) ─────────────────────────
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                 .HasColumnName("PhoneNumber")
                 .HasMaxLength(20);
        });

        builder.Property(u => u.IsActive)
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(u => u.IsLocked)
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // ── Audit fields ─────────────────────────────────────────────────────
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(100);
        builder.Property(u => u.LastModifiedAt);
        builder.Property(u => u.LastModifiedBy).HasMaxLength(100);
        builder.Property(u => u.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(u => u.DeletedAt);
        builder.Property(u => u.DeletedBy).HasMaxLength(100);

        // ── Global query filter: exclude soft-deleted records ─────────────────
        builder.HasQueryFilter(u => !u.IsDeleted);

        // ── Navigation: UserRoles ─────────────────────────────────────────────
        builder.HasMany(u => u.UserRoles)
               .WithOne()
               .HasForeignKey(ur => ur.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
