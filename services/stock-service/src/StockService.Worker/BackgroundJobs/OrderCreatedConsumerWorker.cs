using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockService.Application.Handlers;
using StockService.Infrastructure.Configuration;
using StockService.Infrastructure.Entities;
using StockService.Infrastructure.Events;
using StockService.Infrastructure.Messaging;
using StockService.Infrastructure.Persistence.Context;
using System.Text;
using System.Text.Json;

namespace StockService.Worker.BackgroundJobs
{
    public sealed class OrderCreatedConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly ILogger<OrderCreatedConsumerWorker> _logger;
        private readonly MessagingSettings _settings;
        private readonly NotificationSettings _notificationSettings;

        private IChannel? _channel;
        private string? _consumerTag;
        private const string RetryHeader = "x-retry-count";

        public OrderCreatedConsumerWorker(
            IServiceScopeFactory scopeFactory,
            IConnection connection,
            ILogger<OrderCreatedConsumerWorker> logger,
            ISettings<MessagingSettings> messagingSettings,
            ISettings<NotificationSettings> notificationSettings)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
            _logger = logger;
            _settings = messagingSettings.Value;
            _notificationSettings = notificationSettings.Value;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync(null, stoppingToken);
            var channel = _channel;

            try
            {
                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: _settings.PrefetchCount,
                    global: false,
                    cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: _settings.ExchangeName,
                    type: "topic",
                    durable: true,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: _settings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: _settings.QueueName,
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.OrderCreatedRoutingKey,
                    cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: _settings.DeadLetterExchange,
                    type: "topic",
                    durable: true,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: _settings.DeadLetterQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: _settings.DeadLetterQueue,
                    exchange: _settings.DeadLetterExchange,
                    routingKey: _settings.DeadLetterRoutingKey,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, args) =>
                    await OnMessageReceivedAsync(channel, args, stoppingToken);

                _consumerTag = await channel.BasicConsumeAsync(
                    queue: _settings.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("OrderCreatedConsumerWorker consuming queue {QueueName} with prefetch {Prefetch}", _settings.QueueName, _settings.PrefetchCount);
                await WaitForCancellationAsync(stoppingToken);
            }
            finally
            {
                await CleanupAsync();
            }
        }
        

        private async Task OnMessageReceivedAsync(
            IChannel channel,
            BasicDeliverEventArgs ea,
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
            var handler = scope.ServiceProvider.GetRequiredService<IOrderCreatedMessageHandler>();

            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var envelope = JsonSerializer.Deserialize<EventEnvelope>(json);

            if (envelope == null || envelope.EventId == Guid.Empty || envelope.Data.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("Invalid envelope received. Republishing message on queue {QueueName}", _settings.QueueName);
                await RepublishWithRetryAsync(channel, ea, stoppingToken);
                return;
            }

            var currentRetry = GetRetryCount(ea);
            var attemptNumber = currentRetry + 1;
            var isFinalAttempt = attemptNumber >= _settings.MaxRetryAttempts;

            try
            {
                var alreadyProcessed = await db.ProcessedEvents
                    .AnyAsync(x => x.EventType == envelope.EventType && x.EventId == envelope.EventId && x.Status == ProcessedEventStatus.Completed, stoppingToken);

                if (alreadyProcessed)
                {
                    _logger.LogInformation("OrderId {OrderId} already processed. Acking message.", envelope.EventId);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                var processedEvent = ProcessedEvent.Create(
                    envelope.EventType,
                    _settings.QueueName,
                    envelope.EventId);

                db.ProcessedEvents.Add(processedEvent);

                try
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _logger.LogWarning(
                        ex,
                        "ProcessedEvent insert deduplicated for EventId {EventId}; reusing existing marker",
                        envelope.EventId);
                    db.Entry(processedEvent).State = EntityState.Detached;

                    var existing = await db.ProcessedEvents
                        .AsTracking()
                        .FirstOrDefaultAsync(x => x.EventType == envelope.EventType && x.EventId == envelope.EventId, stoppingToken);

                    if (existing == null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    if (existing.Status == ProcessedEventStatus.Completed)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    if (existing.Status == ProcessedEventStatus.InProgress)
                    {
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                        return;
                    }

                    if (existing.Status == ProcessedEventStatus.Failed)
                    {
                        existing.ResetToInProgress();
                        await db.SaveChangesAsync(stoppingToken);
                        processedEvent = existing;
                    }
                    else
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }
                }
                try
                {
                    await handler.HandleAsync(envelope, isFinalAttempt, stoppingToken);

                    processedEvent.MarkAsProcessed();
                   
                    await db.SaveChangesAsync(stoppingToken);

                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    processedEvent.MarkAsFailed();                   
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogError(ex, "OrderId {OrderId} failed during processing", envelope.EventId);

                    if (_notificationSettings.EnableFailureAlerts)
                    {
                        _logger.LogWarning(
                            "Failure notification scheduled for OrderId {OrderId} on channel {Channel}",
                            envelope.EventId,
                            _notificationSettings.Channel);
                    }

                    await RepublishWithRetryAsync(channel, ea, stoppingToken);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while consuming order message {EventId}", envelope?.EventId);
                await RepublishWithRetryAsync(channel, ea, stoppingToken);
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                return true;
            }

            return false;
        }

        private static int GetRetryCount(BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.TryGetValue(RetryHeader, out var value))
            {
                switch (value)
                {
                    case byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var fromBytes):
                        return fromBytes;
                    case int i:
                        return i;
                    case long l:
                        return (int)l;
                }
            }

            return 0;
        }

        private async Task RepublishWithRetryAsync(
            IChannel channel,
            BasicDeliverEventArgs ea,
            CancellationToken stoppingToken)
        {
            var currentRetry = GetRetryCount(ea);
            var nextRetry = currentRetry + 1;

            var props = new BasicProperties
            {
                ContentType = ea.BasicProperties?.ContentType,
                CorrelationId = ea.BasicProperties?.CorrelationId,
                DeliveryMode = ea.BasicProperties?.DeliveryMode ?? DeliveryModes.Persistent,
                Headers = ea.BasicProperties?.Headers != null
                    ? new Dictionary<string, object?>(ea.BasicProperties.Headers)
                    : new Dictionary<string, object?>()
            };

            props.Headers[RetryHeader] = nextRetry;

            if (nextRetry >= _settings.MaxRetryAttempts)
            {
                _logger.LogWarning("Message exceeded retry count ({RetryCount}) for queue {QueueName}; routing to DLQ", nextRetry, _settings.QueueName);
                await channel.BasicPublishAsync(
                    exchange: _settings.DeadLetterExchange,
                    routingKey: _settings.DeadLetterRoutingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: ea.Body,
                    cancellationToken: stoppingToken);
            }
            else
            {
                await channel.BasicPublishAsync(
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.OrderCreatedRoutingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: ea.Body,
                    cancellationToken: stoppingToken);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        }
        private async Task WaitForCancellationAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Shutdown requested for order consumer; stopping gracefully");
            }
        }

        private async Task CleanupAsync()
        {
            if (_channel == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_consumerTag) && _channel.IsOpen)
                {
                    await _channel.BasicCancelAsync(_consumerTag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error while cancelling consumer for {Queue}", _settings.QueueName);
            }

            try
            {
                if (_channel.IsOpen)
                {
                    await _channel.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error while closing channel for {Queue}", _settings.QueueName);
            }

            _channel.Dispose();
            _channel = null;
            _consumerTag = null;
        }

        private static EventEnvelope? DeserializeEnvelope(string payload)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<EventEnvelope>(payload, options);
        }

        private async Task AckAsync(ulong deliveryTag, CancellationToken ct)
        {
            var channel = _channel;
            if (channel == null)
            {
                return;
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: ct);
        }
    }
}
