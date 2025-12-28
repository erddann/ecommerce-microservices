using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Messaging
{
    public sealed class EventEnvelope
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; } = default!;
        public DateTime OccurredOn { get; init; }
        public JsonElement Data { get; init; }
    }
}
