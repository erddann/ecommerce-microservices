using System;

namespace StockService.Domain.Exceptions
{
    public sealed class InsufficientStockException : Exception
    {
        public InsufficientStockException(Guid productId)
         : base($"Insufficient stock for product {productId}")
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
