using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Infrastructure.DomainFiles.Events;

namespace OrderService.Application.Contracts
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(Guid customerId, IEnumerable<(Guid ProductId, int Quantity)> items, bool saveChanges, CancellationToken cancellationToken);
        Task HandleStockCompletedAsync(OrderStockProcessCompletedDomainEvent domainEvent, CancellationToken cancellationToken);
        Task HandleStockFailedAsync(OrderStockProcessFailedDomainEvent domainEvent, CancellationToken cancellationToken);
    }
}
