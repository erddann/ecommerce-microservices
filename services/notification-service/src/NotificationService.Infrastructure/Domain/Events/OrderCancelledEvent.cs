using NotificationService.Infrastructure.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Domain.Events
{
    public class OrderCancelledEvent : DomainEvent
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string Reason { get; init; } = string.Empty;

        public OrderCancelledEvent(Guid orderId, Guid customerId, string reason = "")
        {
            OrderId = orderId;
            CustomerId = customerId;
            Reason = reason;
        }
    }
}
