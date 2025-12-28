using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Channel).IsRequired();
        builder.Property(x => x.Destination).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.SentOn).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Error).IsRequired(false);
    }
}
