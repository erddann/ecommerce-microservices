using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.Contracts;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.DomainFiles.Events;

namespace OrderService.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateOrderAsync(
            Guid customerId,
            IEnumerable<(Guid ProductId, int Quantity)> items,
            bool saveChanges,
            CancellationToken cancellationToken)
        {
            var order = Order.Create(customerId, items);

            await _orderRepository.AddAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await _outboxRepository.AddAsync(domainEvent, cancellationToken);
            }

            if (saveChanges)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return order.Id;
        }

        public async Task HandleStockCompletedAsync(
            OrderStockProcessCompletedDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for stock completed event", domainEvent.OrderId);
                return;
            }

            await _orderRepository.UpdateStatusAsync(order, OrderStatus.Confirmed, cancellationToken: cancellationToken);

            foreach (var domainEventItem in order.DomainEvents)
            {
                await _outboxRepository.AddAsync(domainEventItem, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task HandleStockFailedAsync(
            OrderStockProcessFailedDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for stock failed event", domainEvent.OrderId);
                return;
            }

            await _orderRepository.UpdateStatusAsync(order, OrderStatus.Cancelled, domainEvent.ErrorMessage, cancellationToken);

            foreach (var domainEventItem in order.DomainEvents)
            {
                await _outboxRepository.AddAsync(domainEventItem, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} cancelled and compensation events recorded", domainEvent.OrderId);
        }
    }
}
