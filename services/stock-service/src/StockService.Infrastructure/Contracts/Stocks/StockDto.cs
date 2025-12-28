using System;

namespace StockService.Infrastructure.Contracts.Stocks
{
    public sealed class StockDto
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
