using StockService.Infrastructure.DomainFiles;
using System;
using System.Collections.Generic;

namespace StockService.Infrastructure.Events
{
    public class OrderStockProcessCompletedDomainEvent : DomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid OrderId { get; init; }
        public List<OrderItemDto> Items { get; init; } = new();
    }
}
