using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Type)
                   .HasColumnName("type")
                   .IsRequired();

            builder.Property(o => o.EventId)
                   .HasColumnName("event_id")
                   .IsRequired();

            builder.Property(o => o.Payload)
                   .HasColumnName("payload")
                   .IsRequired();

            builder.Property(o => o.OccurredOn)
                   .HasColumnName("occurred_on")
                   .IsRequired();

            builder.Property(o => o.ProcessedOn)
                   .HasColumnName("processed_on");
        }
    }
}
