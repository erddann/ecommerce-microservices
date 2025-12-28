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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("id");

            builder.Property(o => o.CustomerId)
                   .HasColumnName("customer_id")
                   .IsRequired();

            builder.Property(o => o.Status)
                   .HasColumnName("status")
                   .IsRequired();

            builder.Property(o => o.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            // Aggregate boundary
            builder.HasMany(typeof(OrderItem), "_items")
                   .WithOne()
                   .HasForeignKey("order_id");

            builder.Navigation("_items")
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
