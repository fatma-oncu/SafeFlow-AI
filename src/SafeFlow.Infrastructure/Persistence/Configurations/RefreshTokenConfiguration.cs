using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Identity.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the <see cref="RefreshToken"/> entity.
/// </summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.UserId).IsRequired();

        builder.Property(rt => rt.TokenHash)
               .HasMaxLength(256)
               .IsRequired();

        // Lookup index: every token validation queries by hash
        builder.HasIndex(rt => rt.TokenHash)
               .IsUnique()
               .HasDatabaseName("IX_RefreshTokens_TokenHash");

        builder.Property(rt => rt.FamilyId).IsRequired();

        // Family index: stolen-token revocation queries all active tokens in a family
        builder.HasIndex(rt => rt.FamilyId)
               .HasDatabaseName("IX_RefreshTokens_FamilyId");

        builder.Property(rt => rt.ExpiresAt).IsRequired();

        builder.Property(rt => rt.RevokedAt);

        builder.Property(rt => rt.ReplacedByTokenHash)
               .HasMaxLength(256);

        builder.Property(rt => rt.CreatedByIp)
               .HasMaxLength(45); // IPv6 max length

        builder.Property(rt => rt.RevokedByIp)
               .HasMaxLength(45);

        // ── Audit fields ─────────────────────────────────────────────────────
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.CreatedBy).HasMaxLength(100);
        builder.Property(rt => rt.LastModifiedAt);
        builder.Property(rt => rt.LastModifiedBy).HasMaxLength(100);
        builder.Property(rt => rt.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(rt => rt.DeletedAt);
        builder.Property(rt => rt.DeletedBy).HasMaxLength(100);

        // ── FK: RefreshToken → User ──────────────────────────────────────────
        builder.HasOne<Domain.Identity.Aggregates.User>()
               .WithMany()
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
