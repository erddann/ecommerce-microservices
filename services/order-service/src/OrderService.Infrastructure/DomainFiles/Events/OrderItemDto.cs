using System;

namespace OrderService.Domain.Events
{
    public class OrderItemDto
    {
        public Guid ProductId { get; }
        public int Quantity { get; }

        public OrderItemDto(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
