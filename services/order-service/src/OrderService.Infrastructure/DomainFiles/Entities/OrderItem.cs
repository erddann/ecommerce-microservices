using System;

namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        private OrderItem() { }

        internal OrderItem(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
