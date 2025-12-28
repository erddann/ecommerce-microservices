using NotificationService.Infrastructure.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Domain.Events
{
    public class OrderConfirmedEvent : DomainEvent
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public decimal OrderTotal { get; init; }

        public OrderConfirmedEvent(Guid orderId, Guid customerId, decimal orderTotal = 0)
        {
            OrderId = orderId;
            CustomerId = customerId;
            OrderTotal = orderTotal;
        }
    }
}
