using System;

namespace StockService.Domain.Exceptions
{
    public sealed class StockNotFoundException : Exception
    {
        public StockNotFoundException(Guid productId)
            : base($"Stock record not found for product {productId}")
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
