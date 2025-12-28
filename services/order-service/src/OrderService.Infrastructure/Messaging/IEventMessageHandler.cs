using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging
{
    /// <summary>
    /// Base abstraction for handling incoming RabbitMQ messages.
    /// Implementers handle specific event types (e.g., OrderStockCompleted, OrderStockFail).
    /// </summary>
    public interface IEventMessageHandler
    {
        /// <summary>
        /// The queue name this handler consumes from.
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Handle the incoming message payload.
        /// Should return success status; if returns false, message remains unacked (for requeue/DLQ).
        /// </summary>
        Task<bool> HandleAsync(string payload, CancellationToken ct);
    }
}
