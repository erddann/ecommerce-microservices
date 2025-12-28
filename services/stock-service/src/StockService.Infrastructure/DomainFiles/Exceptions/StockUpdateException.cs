using System;

namespace StockService.Domain.Exceptions
{
    public sealed class StockUpdateException : Exception
    {
        public StockUpdateException(Guid productId, Exception inner)
            : base($"Stock update failed for product {productId}", inner)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
