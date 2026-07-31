using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

internal sealed class IncidentCommentConfiguration : IEntityTypeConfiguration<IncidentComment>
{
    public void Configure(EntityTypeBuilder<IncidentComment> builder)
    {
        builder.ToTable("IncidentComments", "incident");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.IncidentId).IsRequired();
        builder.Property(c => c.AuthorEmployeeId).IsRequired();
        builder.Property(c => c.Content).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
