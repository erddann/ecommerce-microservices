using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OrderService.Application.Handlers;
using OrderService.Infrastructure.Entities;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence.Context;
using OrderService.Infrastructure.Settings;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OrderService.Worker.BackgroundJobs
{
    public class OrderStockCompletedWorker : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderStockCompletedWorker> _logger;
        private readonly MessagingSettings _settings;

        private string ExchangeName => _settings.IncomingStock.Exchange;
        private string QueueName => _settings.IncomingStock.CompletedQueue;
        private string RoutingKey => _settings.IncomingStock.CompletedRoutingKey;

        private IChannel? _channel;
        private string? _consumerTag;

        public OrderStockCompletedWorker(
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderStockCompletedWorker> logger,
            ISettings<MessagingSettings> settings)
        {
            _connection = connection;
            _scopeFactory = scopeFactory ;
            _logger = logger ;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await _channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    cancellationToken: stoppingToken);

                await _channel.BasicQosAsync(0, 10, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (_, ea) =>
                    await OnMessageReceivedAsync(_channel, ea, stoppingToken);

                _consumerTag = await _channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("OrderStockCompleted consumer listening on {QueueName}", QueueName);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("OrderStockCompleted consumer stopping (cancellation requested)");
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
            try
            {
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var handler = scope.ServiceProvider.GetRequiredService<IOrderStockCompletedMessageHandler>();

                var envelope = JsonSerializer.Deserialize<EventEnvelope>(payload);
                if (envelope == null || envelope.EventId == Guid.Empty || envelope.Data.ValueKind == JsonValueKind.Undefined)
                {
                    _logger.LogWarning("Invalid envelope for OrderStockCompleted message: {Payload}", payload);
                    // await RepublishWithRetryAsync(channel, ea, stoppingToken);
                    return;
                }

                // Check existing record status
                var alreadyProcessed = await db.ProcessedEvents
                    .AnyAsync(x => x.EventType == envelope.EventType && x.EventId == envelope.EventId && x.Status == ProcessedEventStatus.Completed, stoppingToken);

                if (alreadyProcessed)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                var processedEvent = ProcessedEvent.Create(
                    envelope.EventType,
                    QueueName,
                    envelope.EventId);

                db.ProcessedEvents.Add(processedEvent);

                try
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _logger.LogWarning(ex, "Unique constraint violation while tracking ProcessedEvent for {EventId}", envelope.EventId);
                    db.Entry(processedEvent).State = EntityState.Detached;

                    var existing = await db.ProcessedEvents
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
                    var handled = await handler.HandleAsync(envelope, isFinalAttempt: false, cancellationToken: CancellationToken.None);
                    if (!handled)
                    {
                        _logger.LogWarning("OrderStockCompleted handler returned unsuccessful result for {EventId}", envelope.EventId);
                    }

                    processedEvent.MarkAsProcessed();
                    await db.SaveChangesAsync(stoppingToken);

                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch
                {
                    _logger.LogError("Error handling OrderStockCompleted event {EventId}, marking failed and requeueing", envelope.EventId);
                    processedEvent.MarkAsFailed();
                    await db.SaveChangesAsync(stoppingToken);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while processing OrderStockCompleted message {DeliveryTag}", ea.DeliveryTag);
                try
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
                catch (Exception nackEx)
                {
                    _logger.LogWarning(nackEx, "Failed to nack OrderStockCompleted message {DeliveryTag}", ea.DeliveryTag);
                }
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

        private async Task CleanupAsync()
        {
            try
            {
                if (_consumerTag != null && _channel?.IsOpen == true)
                {
                    await _channel.BasicCancelAsync(_consumerTag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling OrderStockCompleted consumer on queue {QueueName}", QueueName);
            }

            try
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing channel for OrderStockCompleted queue {QueueName}", QueueName);
            }

            _channel?.Dispose();
        }
    }
}
