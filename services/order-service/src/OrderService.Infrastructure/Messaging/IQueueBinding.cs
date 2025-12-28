using System;

namespace OrderService.Infrastructure.Messaging
{
    public interface IQueueBinding
    {
        string ExchangeName { get; }
        string RoutingKey { get; }
    }
}
