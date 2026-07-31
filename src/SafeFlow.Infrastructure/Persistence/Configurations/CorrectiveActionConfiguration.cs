using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Infrastructure.Persistence.Configurations;

internal sealed class CorrectiveActionConfiguration : IEntityTypeConfiguration<CorrectiveAction>
{
    public void Configure(EntityTypeBuilder<CorrectiveAction> builder)
    {
        builder.ToTable("CorrectiveActions", "incident");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.IncidentId).IsRequired();

        builder.OwnsOne(c => c.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.Property(c => c.AssignedToEmployeeId).IsRequired();
        builder.Property(c => c.DueDate).IsRequired();
        builder.Property(c => c.CompletedAt);
        builder.Property(c => c.CompletedByEmployeeId);
        builder.Property(c => c.Status)
               .HasConversion<int>()
               .IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
