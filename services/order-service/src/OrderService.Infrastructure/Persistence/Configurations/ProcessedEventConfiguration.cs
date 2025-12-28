using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Configurations
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.EventType)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.QueueName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.EventId)
                .IsRequired();

            builder.Property(p => p.OccurredOn)
                .IsRequired();

            builder.Property(p => p.ProcessedOn)
                .IsRequired(false)  // nullable - null = in-flight
                .HasColumnType("timestamp with time zone");  // PostgreSQL specific (if needed)

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(ProcessedEventStatus.InProgress);

            // ⭐ CRITICAL: Unique constraint on (EventType, EventId)
            // Acts as distributed lock: first INSERT wins, subsequent duplicates fail
            builder.HasIndex(p => new { p.EventType, p.EventId })
                .IsUnique()
                .HasDatabaseName("IX_ProcessedEvent_EventType_EventId");

            // Index for querying successfully processed events only
            builder.HasIndex(p => new { p.QueueName, p.ProcessedOn })
                .HasDatabaseName("IX_ProcessedEvent_QueueName_ProcessedOn");
        }
    }
}
