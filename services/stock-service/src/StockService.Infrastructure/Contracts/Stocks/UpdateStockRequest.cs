using System.ComponentModel.DataAnnotations;

namespace StockService.Infrastructure.Contracts.Stocks
{
    public sealed class UpdateStockRequest
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
