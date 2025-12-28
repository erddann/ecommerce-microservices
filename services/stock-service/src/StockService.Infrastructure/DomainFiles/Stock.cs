using System;

namespace StockService.Domain.Entities
{
    public class Stock
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        protected Stock() { }

        public Stock(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
        }
    }
}
