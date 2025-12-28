using System;using System.Collections.Generic;using System.Linq;using OrderService.Domain.Common;using OrderService.Domain.Enums;using OrderService.Domain.Events;using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities
{
    public class Order : AggregateRoot
    {
        public readonly List<OrderItem> _items = new List<OrderItem>();
        public Guid Id { get; set; }
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Order() { }

        private Order(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new DomainException("CustomerId cannot be empty.");

            Id = Guid.NewGuid();
            CustomerId = customerId;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
        }

        public static Order Create(
            Guid customerId,
            IEnumerable<(Guid ProductId, int Quantity)> items)
        {
            var order = new Order(customerId);

            foreach (var item in items)
            {
                order.AddItem(item.ProductId, item.Quantity);
            }

            order.AddDomainEvent(
                new OrderCreatedEvent(
                    order.Id,
                    order._items.Select(i => new OrderItemDto(i.ProductId, i.Quantity)).ToList())                
                    );

            return order;
        }

        private void AddItem(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero.");

            _items.Add(new OrderItem(productId, quantity));
        }
    }
}
