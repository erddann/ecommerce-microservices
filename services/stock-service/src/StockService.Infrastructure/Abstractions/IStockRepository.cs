using StockService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Abstractions
{
    public interface IStockRepository : IGenericRepository<Stock>
    {
        Task DecreaseStockAtomicallyAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken);

        Task DecreaseStockBatchAsync(
            IEnumerable<(Guid productId, int quantity)> items,
            CancellationToken cancellationToken);
    }
}
