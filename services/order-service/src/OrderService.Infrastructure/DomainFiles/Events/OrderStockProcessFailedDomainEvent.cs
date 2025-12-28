using OrderService.Domain.Common;
using OrderService.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.DomainFiles.Events
{
    public class OrderStockProcessFailedDomainEvent:DomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid OrderId { get; init; }
        public List<OrderItemDto> Items { get; init; } = new();
        public string ErrorMessage { get; init; } = string.Empty;
    }
}
