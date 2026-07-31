using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

internal sealed class IncidentAttachmentConfiguration : IEntityTypeConfiguration<IncidentAttachment>
{
    public void Configure(EntityTypeBuilder<IncidentAttachment> builder)
    {
        builder.ToTable("IncidentAttachments", "incident");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.IncidentId).IsRequired();
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileUrl).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.SizeBytes).IsRequired();
        builder.Property(a => a.UploadedByEmployeeId).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
    }
}
