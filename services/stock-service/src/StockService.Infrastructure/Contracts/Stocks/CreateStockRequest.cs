using System;
using System.ComponentModel.DataAnnotations;

namespace StockService.Infrastructure.Contracts.Stocks
{
    public sealed class CreateStockRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
