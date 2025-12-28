using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging
{
    public interface IEventBus
    {
        Task PublishAsync(string eventType, string payload, CancellationToken ct);
    }
}
