using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Generic RabbitMQ consumer worker that subscribes to a queue and delegates message handling
    /// to a registered IEventMessageHandler.
    /// 
    /// Key Design:
    /// - One instance per handler type (one per queue)
    /// - Long-lived channel per worker
    /// - Fresh scope per message (clean dependencies)
    /// - Proper async message handling (AsyncEventingBasicConsumer with await)
    /// - Graceful shutdown with channel cleanup
    /// </summary>
    public class RabbitMqConsumerWorker : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Type _handlerType;
        private readonly ILogger<RabbitMqConsumerWorker> _logger;
        private readonly string _exchangeName;
        private readonly SemaphoreSlim _processingLock = new(1, 1);

        private IChannel? _channel;
        private string? _queueName;
        private string? _exchangeOverride;
        private string? _routingKeyOverride;
        private DelegatingAsyncConsumer? _consumer;
        private string? _consumerTag;

        public RabbitMqConsumerWorker(
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            Type handlerType,
            ILogger<RabbitMqConsumerWorker> logger,
            string? exchangeName = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _handlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exchangeName = exchangeName ?? "order.events.topic";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Step 1: Get queue name (resolve handler once, extract queue name, dispose scope)
                _queueName = GetQueueName();
                _logger.LogInformation("RabbitMqConsumerWorker starting for queue: {QueueName}", _queueName);

                // Step 2: Setup channel with QoS and declare queue
                await SetupChannelAndTopologyAsync(stoppingToken);

                // Step 3: Setup consumer
                await SetupConsumerAsync(stoppingToken);

                _logger.LogInformation("RabbitMqConsumerWorker listening on queue: {QueueName}", _queueName);

                // Step 4: keep running until cancellation
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("RabbitMqConsumerWorker stopping (cancellation requested)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in RabbitMqConsumerWorker for queue {QueueName}", _queueName);
                throw;
            }
            finally
            {
                await CleanupAsync();
            }
        }

        /// <summary>
        /// Extract queue name from handler (one-time operation with scoped lifetime).
        /// Scope is disposed immediately after.
        /// </summary>
        private string GetQueueName()
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(_handlerType)
                as IEventMessageHandler
                ?? throw new InvalidOperationException(
                    $"Handler {_handlerType.Name} does not implement IEventMessageHandler");

            if (handler is IQueueBinding binding)
            {
                _exchangeOverride = binding.ExchangeName;
                _routingKeyOverride = binding.RoutingKey;
            }

            return handler.QueueName;
        }

        /// <summary>
        /// Setup channel with QoS settings and declare queue (idempotent).
        /// </summary>
        private async Task SetupChannelAndTopologyAsync(CancellationToken stoppingToken)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // QoS: prefetch 10 messages per consumer
                await _channel.BasicQosAsync(0, 10, false, stoppingToken);

                // Determine exchange and routing key (handler override if provided)
                var exchange = _exchangeOverride ?? _exchangeName;

                // Declare dedicated topic exchange to avoid conflicts with existing fanout
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    cancellationToken: stoppingToken);

                // Declare queue (idempotent)
                var queueName = _queueName ?? throw new InvalidOperationException("Queue name is not initialized");

                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                // Bind queue to exchange using queue name (event type) as routing key
                var routingKey = _routingKeyOverride ?? queueName;
                await _channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: routingKey,
                    cancellationToken: stoppingToken);

                _logger.LogDebug("Channel setup and queue declared for: {QueueName}", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to setup channel and topology for queue: {QueueName}", _queueName);
                throw;
            }
        }

        /// <summary>
        /// Setup async consumer that delegates to our handler.
        /// </summary>
        private async Task SetupConsumerAsync(CancellationToken stoppingToken)
        {
            try
            {
                var channel = _channel ?? throw new InvalidOperationException("Channel is not initialized");
                var queueName = _queueName ?? throw new InvalidOperationException("Queue name is not initialized");

                _consumer = new DelegatingAsyncConsumer(channel, ea => OnMessageReceivedAsync(ea, stoppingToken));

                _consumerTag = await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumerTag: string.Empty,
                    noLocal: false,
                    exclusive: false,
                    arguments: null,
                    consumer: _consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Consumer registered for queue: {QueueName}", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to setup consumer for queue: {QueueName}", _queueName);
                throw;
            }
        }

        /// <summary>
        /// Handle incoming message with fresh scoped handler.
        /// Each message gets its own scope to ensure clean dependencies (fresh DbContext, Logger, etc).
        /// </summary>
        private async Task OnMessageReceivedAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            await _processingLock.WaitAsync(stoppingToken);
            try
            {
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogDebug("Message received on queue {QueueName}, DeliveryTag: {DeliveryTag}",
                    _queueName, ea.DeliveryTag);

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService(_handlerType)
                    as IEventMessageHandler
                    ?? throw new InvalidOperationException(
                        $"Handler {_handlerType.Name} does not implement IEventMessageHandler");

                var success = await handler.HandleAsync(payload, stoppingToken);

                if (success)
                {
                    await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    _logger.LogDebug("Message {DeliveryTag} acknowledged on queue {QueueName}", ea.DeliveryTag, _queueName);
                }
                else
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    _logger.LogWarning("Message {DeliveryTag} rejected (nack) on queue {QueueName} - will be sent to DLQ", ea.DeliveryTag, _queueName);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Message processing cancelled");
                try
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: CancellationToken.None);
                }
                catch (Exception nackEx)
                {
                    _logger.LogWarning(nackEx, "Failed to nack message {DeliveryTag} during cancellation", ea.DeliveryTag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {DeliveryTag} on queue {QueueName}", ea.DeliveryTag, _queueName);
                try { await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None); } catch (Exception nackEx) { _logger.LogError(nackEx, "Failed to nack message {DeliveryTag}", ea.DeliveryTag); }
            }
            finally
            {
                _processingLock.Release();
            }
        }

        /// <summary>
        /// Graceful cleanup: stop consuming, close channel.
        /// </summary>
        private async Task CleanupAsync()
        {
            try
            {
                if (_consumerTag != null && _channel?.IsOpen == true)
                {
                    await _channel.BasicCancelAsync(_consumerTag);
                    _logger.LogInformation("Stopped consuming from queue {QueueName}", _queueName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping consumer for queue {QueueName}", _queueName);
            }

            try
            {
                if (_channel != null)
                {
                    try
                    {
                        await _channel.CloseAsync();
                    }
                    catch (Exception closeEx)
                    {
                        _logger.LogWarning(closeEx, "Error closing channel for queue {QueueName}", _queueName);
                    }
                    _logger.LogDebug("Channel closed for queue {QueueName}", _queueName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing channel for queue {QueueName}", _queueName);
            }
            finally
            {
                _channel?.Dispose();
            }

            _logger.LogInformation("RabbitMqConsumerWorker cleanup complete for queue {QueueName}", _queueName);
        }

        /// <summary>
        /// Simple async consumer that delegates deliveries to a provided handler.
        /// </summary>
        private sealed class DelegatingAsyncConsumer : IAsyncBasicConsumer
        {
            private readonly Func<BasicDeliverEventArgs, Task> _handler;

            public DelegatingAsyncConsumer(IChannel channel, Func<BasicDeliverEventArgs, Task> handler)
            {
                Channel = channel;
                _handler = handler;
            }

            public IChannel Channel { get; }

            public event AsyncEventHandler<ConsumerEventArgs>? ConsumerCancelled;
            public event AsyncEventHandler<ConsumerEventArgs>? Registered;
            public event AsyncEventHandler<ConsumerEventArgs>? Unregistered;
            public event AsyncEventHandler<ShutdownEventArgs>? Shutdown;

            public Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
                => ConsumerCancelled != null ? ConsumerCancelled.Invoke(this, new ConsumerEventArgs(new[] { consumerTag })) : Task.CompletedTask;

            public Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken = default)
                => Registered != null ? Registered.Invoke(this, new ConsumerEventArgs(new[] { consumerTag })) : Task.CompletedTask;

            public Task HandleBasicDeliverAsync(
                string consumerTag,
                ulong deliveryTag,
                bool redelivered,
                string exchange,
                string routingKey,
                IReadOnlyBasicProperties properties,
                ReadOnlyMemory<byte> body,
                CancellationToken cancellationToken = default)
            {
                var ea = new BasicDeliverEventArgs(
                    consumerTag,
                    deliveryTag,
                    redelivered,
                    exchange,
                    routingKey,
                    properties,
                    body,
                    cancellationToken);

                return _handler(ea);
            }

            public Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
                => Shutdown != null ? Shutdown.Invoke(this, reason) : Task.CompletedTask;

            public Task HandleConsumerUnregisteredAsync(string consumerTag, CancellationToken cancellationToken = default)
                => Unregistered != null ? Unregistered.Invoke(this, new ConsumerEventArgs(new[] { consumerTag })) : Task.CompletedTask;

            public Task HandleConsumerRegisteredAsync(string consumerTag, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
