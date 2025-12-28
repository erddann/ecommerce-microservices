using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Abstractions
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task UpdateStatusAsync(Order order, OrderStatus status, string? reason = null, CancellationToken cancellationToken = default);
    }
}
