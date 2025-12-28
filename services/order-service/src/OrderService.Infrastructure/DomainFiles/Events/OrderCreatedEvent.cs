using System;
using System.Collections.Generic;
using OrderService.Domain.Common;

namespace OrderService.Domain.Events
{
    public class OrderCreatedEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public IReadOnlyCollection<OrderItemDto> Items { get; }

        public OrderCreatedEvent(
            Guid orderId,
            IReadOnlyCollection<OrderItemDto> items)
        {
            OrderId = orderId;
            Items = items;
        }
    }
}
