using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(OrderDbContext context)
            : base(context)
        {
        }

        public Task UpdateStatusAsync(Order order, Domain.Enums.OrderStatus status, string? information = null, CancellationToken cancellationToken = default)
        {
            var entry = Context.Entry(order);
            if (entry.State == EntityState.Detached)
            {
                Context.Attach(order);
                entry = Context.Entry(order);
            }

            // Update status via context to avoid invoking aggregate methods directly
            entry.Property(nameof(Order.Status)).CurrentValue = status;

            // Manually append corresponding domain event
            Domain.Common.DomainEvent? domainEvent = status switch
            {
                Domain.Enums.OrderStatus.Confirmed => new Domain.Events.OrderConfirmedEvent(order.Id, order.CustomerId),
                Domain.Enums.OrderStatus.Cancelled => new Domain.Events.OrderCancelledEvent(order.Id, order.CustomerId, information ?? "Cancelled"),
                _ => null
            };

            if (domainEvent != null)
            {
                var addMethod = typeof(Domain.Common.AggregateRoot)
                    .GetMethod("AddDomainEvent", BindingFlags.Instance | BindingFlags.NonPublic);
                addMethod?.Invoke(order, new object[] { domainEvent });
            }

            return Task.CompletedTask;
        }

    }
}
