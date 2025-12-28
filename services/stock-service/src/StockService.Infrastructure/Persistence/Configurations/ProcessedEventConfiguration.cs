using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Infrastructure.Entities;

namespace StockService.Infrastructure.Persistence.Configurations
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.ToTable("processed_events");

            builder.HasKey(x => x.EventId);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.QueueName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.ProcessedOn)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(ProcessedEventStatus.InProgress);

            builder.HasIndex(x => new { x.EventType, x.EventId })
                .IsUnique();

            builder.HasIndex(x => new { x.QueueName, x.ProcessedOn });
        }
    }
}
