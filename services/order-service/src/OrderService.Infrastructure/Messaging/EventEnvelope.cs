using System;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging
{
    public class EventEnvelope
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; }
        public DateTime OccurredOn { get; init; }
        public JsonElement Data { get; init; }
    }
}
