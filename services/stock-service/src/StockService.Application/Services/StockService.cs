using Microsoft.Extensions.Logging;
using StockService.Application.Contracts;
using StockService.Domain.Entities;
using StockService.Domain.Exceptions;
using StockService.Infrastructure.Abstractions;
using StockService.Infrastructure.Contracts.Stocks;
using StockService.Infrastructure.Events;
using StockService.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace StockService.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IStockRepository stockRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork,
            ILogger<StockService> logger)
        {
            _stockRepository = stockRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<StockDto>> GetStocksAsync(CancellationToken cancellationToken)
        {
            var stocks = await _stockRepository.GetAllAsync(cancellationToken);

            return stocks.Select(MapToDto);
        }

        public async Task<StockDto?> GetStockAsync(Guid productId, CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(productId, cancellationToken);

            return stock == null ? null : MapToDto(stock);
        }

        public async Task<StockDto> CreateStockAsync(
            CreateStockRequest request,
            CancellationToken cancellationToken)
        {
            var stock = new Stock(request.ProductId, request.Quantity);
            await _stockRepository.AddAsync(stock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock created for ProductId {ProductId}", stock.ProductId);
            return MapToDto(stock);
        }

        public async Task<bool> UpdateStockAsync(
            Guid productId,
            UpdateStockRequest request,
            CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(productId, cancellationToken);
            if (stock == null)
            {
                return false;
            }

            stock.UpdateQuantity(request.Quantity);
            await _stockRepository.UpdateAsync(stock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock updated for ProductId {ProductId}", productId);
            return true;
        }

        public async Task<bool> DeleteStockAsync(Guid productId, CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(productId, cancellationToken);
            if (stock == null)
            {
                return false;
            }

            await _stockRepository.DeleteAsync(stock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock deleted for ProductId {ProductId}", productId);
            return true;
        }

        public async Task HandleOrderCreatedAsync(
            EventEnvelope envelope,
            bool isFinalAttempt,
            CancellationToken cancellationToken)
        {
            var order = GetOrderData(envelope.Data);
            _logger.LogInformation("Handling order stock request for OrderId {OrderId}", order.OrderId);

            try
            {
                var items = order.Items
                    .Select(x => (x.ProductId, x.Quantity))
                    .ToList();

                await _stockRepository.DecreaseStockBatchAsync(
                    items,
                    cancellationToken);

                var completedEvent = new OrderStockProcessCompletedDomainEvent
                {
                    OrderId = order.OrderId,
                    Items = order.Items
                };

                await _outboxRepository.AddAsync(completedEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Stock processing completed for OrderId {OrderId}", order.OrderId);
            }
            catch (Exception ex)
            {
                var errorType = ex switch
                {
                    StockNotFoundException => nameof(StockNotFoundException),
                    InsufficientStockException => nameof(InsufficientStockException),
                    StockUpdateException => nameof(StockUpdateException),
                    _ => "Unexpected"
                };

                if (ex is StockNotFoundException or InsufficientStockException)
                {
                    _logger.LogWarning(ex, "OrderId {OrderId} failed due to {ErrorType}", order.OrderId, errorType);
                }
                else
                {
                    _logger.LogError(ex, "OrderId {OrderId} failed while processing stock", order.OrderId);
                }

                if (isFinalAttempt)
                {
                    var failedEvent = new OrderStockProcessFailedDomainEvent
                    {
                        OrderId = order.OrderId,
                        Items = order.Items,
                        ErrorMessage = errorType
                    };

                    await _outboxRepository.AddAsync(failedEvent, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogWarning("Fail event enqueued for OrderId {OrderId} with error {ErrorType}", order.OrderId, errorType);
                }

                throw;
            }
        }

        private static StockDto MapToDto(Stock stock)
        {
            return new StockDto
            {
                ProductId = stock.ProductId,
                Quantity = stock.Quantity
            };
        }

        private static OrderCreatedEventDto GetOrderData(JsonElement data)
        {
            return data.ValueKind == JsonValueKind.String
                ? JsonSerializer.Deserialize<OrderCreatedEventDto>(data.GetString()!)!
                : data.Deserialize<OrderCreatedEventDto>()!;
        }
    }

}
