using System;
using OrderService.Domain.Common;

namespace OrderService.Domain.Events
{
    public class OrderConfirmedEvent : DomainEvent
    {
        public Guid OrderId { get; }

        public OrderConfirmedEvent(Guid orderId, Guid customerId, decimal orderTotal = 0)
        {
            OrderId = orderId;
            OrderTotal = orderTotal;
            CustomerId = customerId;
        }
        public Guid CustomerId  { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
