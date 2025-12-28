using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Entities
{
    public class ProcessedEvent
    {
        private ProcessedEvent() { }

        public Guid Id { get; private set; }
        public string EventType { get; private set; } = default!;
        public string QueueName { get; private set; } = default!;
        public Guid EventId { get; private set; }
        public DateTime OccurredOn { get; private set; }

        public ProcessedEventStatus Status { get; private set; }

        /// <summary>
        /// null = Lock acquired but processing in-flight
        /// timestamp = Successfully processed
        /// </summary>
        public DateTime? ProcessedOn { get; private set; }

        /// <summary>
        /// Creates a ProcessedEvent with lock (ProcessedOn = null).
        /// ProcessedOn will be set after successful processing.
        /// </summary>
        public static ProcessedEvent Create(string eventType, string queueName, Guid eventId)
        {
            return new ProcessedEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                QueueName = queueName,
                EventId = eventId,
                OccurredOn = DateTime.UtcNow,
                ProcessedOn = null,
                Status = ProcessedEventStatus.InProgress
            };
        }

        /// <summary>
        /// Marks event as successfully processed.
        /// Call this only after all side effects (Order update, OutboxMessages) are saved.
        /// </summary>
        public void MarkAsProcessed()
        {
            Status = ProcessedEventStatus.Completed;
            ProcessedOn = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            Status = ProcessedEventStatus.Failed;
            ProcessedOn = null;
        }

        public void ResetToInProgress()
        {
            Status = ProcessedEventStatus.InProgress;
            ProcessedOn = null;
        }
    }
}
