using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.QueueName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.OccurredOn).IsRequired();
        builder.Property(x => x.ProcessedOn);

        builder.HasIndex(x => new { x.EventId, x.QueueName }).IsUnique();
    }
}
