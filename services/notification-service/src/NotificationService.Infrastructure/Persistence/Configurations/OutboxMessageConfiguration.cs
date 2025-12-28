using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.OccurredOn).IsRequired();
        builder.Property(x => x.Type).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Data)
            .HasConversion(
                v => v.GetRawText(),
                v => System.Text.Json.JsonDocument.Parse(v, new System.Text.Json.JsonDocumentOptions()).RootElement.Clone())
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.ProcessedOn).HasFilter("[ProcessedOn] IS NULL");
    }
}
