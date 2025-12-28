using System;
using OrderService.Domain.Common;

namespace OrderService.Domain.Events
{
    public class OrderCancelledEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public string Reason { get; set; }
        public Guid CustomerId { get; set; }

        public OrderCancelledEvent(Guid orderId, Guid customerId , string reason = "")
        {
            OrderId = orderId;
            Reason = reason;
            CustomerId = customerId;
        }
    }
}
