using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Messaging
{
    public interface IEventBus
    {
        Task PublishAsync(
            string exchangeName,
            string eventType,
            string payload,
            CancellationToken ct);
    }
}
