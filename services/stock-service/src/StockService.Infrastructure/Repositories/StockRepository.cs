using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using StockService.Domain.Entities;
using StockService.Domain.Exceptions;
using StockService.Infrastructure.Abstractions;
using StockService.Infrastructure.Persistence.Context;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Repositories
{
    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        private readonly ILogger<StockRepository> _logger;

        public StockRepository(StockDbContext db, ILogger<StockRepository> logger)
            : base(db)
        {
            _logger = logger;
        }

        public async Task DecreaseStockAtomicallyAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken)
        {
            int affected;

            try
            {
                affected =
                    await DbContext.Database.ExecuteSqlRawAsync(
                        """
                    UPDATE "stocks"
                    SET "Quantity" = "Quantity" - {1}
                    WHERE "ProductId" = {0}
                      AND "Quantity" >= {1}
                    """,
                        new object[] { productId, quantity },
                        cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug(ex, "Stock update cancelled for product {ProductId}", productId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Stock update failed for product {ProductId} due to database update error", productId);
                throw new StockUpdateException(productId, ex);
            }
            catch (PostgresException ex)
            {
                _logger.LogError(ex, "Stock update failed for product {ProductId} due to postgres error", productId);
                throw new StockUpdateException(productId, ex);
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Stock update failed for product {ProductId} due to db error", productId);
                throw new StockUpdateException(productId, ex);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unexpected stock update failure for product {ProductId}", productId);
                throw new StockUpdateException(productId, ex);
            }

            if (affected == 0)
            {
                var exists = await DbSet
                    .AnyAsync(x => x.ProductId == productId, cancellationToken);

                if (!exists)
                {
                    _logger.LogWarning("Stock not found for product {ProductId} during decrease", productId);
                    throw new StockNotFoundException(productId);
                }

                _logger.LogWarning("Insufficient stock for product {ProductId}", productId);
                throw new InsufficientStockException(productId);
            }
            else
            {
                _logger.LogInformation("Stock decreased for product {ProductId} by {Quantity}", productId, quantity);
            }
        }

        public async Task DecreaseStockBatchAsync(
            IEnumerable<(Guid productId, int quantity)> items,
            CancellationToken cancellationToken)
        {
            var itemList = items.ToList();
            await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Starting batch stock decrease for {ItemCount} items", itemList.Count);
                foreach (var (productId, quantity) in itemList)
                {
                    await DecreaseStockAtomicallyAsync(
                        productId,
                        quantity,
                        cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Completed batch stock decrease successfully");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Stock decrease batch cancelled; transaction rolled back");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Stock decrease batch failed; transaction rolled back");
                throw;
            }
        }
    }

}
