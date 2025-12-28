using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TemplateCode).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Channel).IsRequired();
        builder.Property(x => x.Language).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Subject).IsRequired();
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.Version).IsRequired();

        builder.HasIndex(x => new { x.TemplateCode, x.Channel, x.Language }).IsUnique();
    }
}
