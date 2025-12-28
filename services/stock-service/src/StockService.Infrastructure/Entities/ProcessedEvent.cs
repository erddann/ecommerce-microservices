using System;

namespace StockService.Infrastructure.Entities
{
    public class ProcessedEvent
    {
        private ProcessedEvent() { }

        public Guid EventId { get; private set; }
        public string EventType { get; private set; } = default!;
        public string QueueName { get; private set; } = default!;
        public DateTime? ProcessedOn { get; private set; }
        public ProcessedEventStatus Status { get; private set; }
        public DateTime OccurredOn { get; private set; }

        public static ProcessedEvent Create(
            string eventType,
            string queueName,
            Guid eventId)
        {
            return new ProcessedEvent
            {
                EventType = eventType,
                QueueName = queueName,
                EventId = eventId,
                OccurredOn = DateTime.UtcNow,
                Status = ProcessedEventStatus.InProgress
            };
        }

        public void MarkAsProcessed()
        {
            ProcessedOn = DateTime.UtcNow;
            Status = ProcessedEventStatus.Completed;
        }

        public void MarkAsFailed()
        {
            ProcessedOn = null;
            Status = ProcessedEventStatus.Failed;
        }

        public void ResetToInProgress()
        {
            ProcessedOn = null;
            Status = ProcessedEventStatus.InProgress;
        }
    }
}
