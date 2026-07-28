using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Infrastructure.Identity;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for <see cref="ApplicationUser"/>.
/// </summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Identity already creates the AspNetUsers table; we just extend its columns.
        builder.Property(u => u.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(u => u.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(u => u.TenantId)
               .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // Tenant + email lookup index
        builder.HasIndex(u => new { u.TenantId, u.Email })
               .HasDatabaseName("IX_AspNetUsers_TenantId_Email");
    }
}
