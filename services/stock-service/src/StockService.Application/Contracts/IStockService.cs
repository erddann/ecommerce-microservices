using StockService.Infrastructure.Contracts.Stocks;
using StockService.Infrastructure.Messaging;
using System;
using System.Collections.Generic;

namespace StockService.Application.Contracts
{
    public interface IStockService
    {
        Task<IEnumerable<StockDto>> GetStocksAsync(CancellationToken cancellationToken);
        Task<StockDto?> GetStockAsync(Guid productId, CancellationToken cancellationToken);
        Task<StockDto> CreateStockAsync(CreateStockRequest request, CancellationToken cancellationToken);
        Task<bool> UpdateStockAsync(Guid productId, UpdateStockRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteStockAsync(Guid productId, CancellationToken cancellationToken);

        Task HandleOrderCreatedAsync(
            EventEnvelope envelope,
            bool isFinalAttempt,
            CancellationToken cancellationToken);
    }
}
