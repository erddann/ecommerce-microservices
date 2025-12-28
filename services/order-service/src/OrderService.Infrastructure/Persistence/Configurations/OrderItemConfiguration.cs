using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("id");

            builder.Property<Guid>("order_id")
                   .HasColumnName("order_id")
                   .IsRequired();

            builder.Property(o => o.ProductId)
                   .HasColumnName("product_id")
                   .IsRequired();

            builder.Property(o => o.Quantity)
                   .HasColumnName("quantity")
                   .IsRequired();

            builder.HasIndex("order_id", nameof(OrderItem.ProductId))
                   .IsUnique();
        }
    }
}
