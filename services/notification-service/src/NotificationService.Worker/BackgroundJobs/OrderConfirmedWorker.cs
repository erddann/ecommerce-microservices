using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Handlers;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Domain;
using NotificationService.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Worker.BackgroundJobs;

public class OrderConfirmedWorker : BackgroundService
{
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqOptions _options;
    private readonly NotificationSettings _settings;
    private readonly ILogger<OrderConfirmedWorker> _logger;
    private IChannel? _channel;
    private string? _consumerTag;

    public OrderConfirmedWorker(
        RabbitMqConnectionProvider connectionProvider,
        IServiceProvider serviceProvider,
        RabbitMqOptions options,
        ISettings<NotificationSettings> settings,
        ILogger<OrderConfirmedWorker> logger)
    {
        _connectionProvider = connectionProvider;
        _serviceProvider = serviceProvider;
        _options = options;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var connection = _connectionProvider.GetConnection();
            _channel = await connection.CreateChannelAsync(null, stoppingToken);

            var queueName = _options.OrderConfirmedQueue;
            var routingKey = _options.OrderConfirmedRoutingKey;
            _logger.LogInformation("Starting OrderConfirmed consumer for queue {Queue}", queueName);

            await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(queueName, _options.ExchangeName, routingKey, arguments: null, cancellationToken: stoppingToken);
            var prefetchCount = _settings.PrefetchCount == 0 ? (ushort)1 : _settings.PrefetchCount;
            await _channel.BasicQosAsync(0, prefetchCount, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (_, args) => HandleMessageAsync(args, queueName, stoppingToken);

            _consumerTag = await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, consumerTag: string.Empty, noLocal: false, exclusive: false, arguments: null, cancellationToken: stoppingToken);
            _logger.LogInformation("OrderConfirmed consumer subscribed with tag {ConsumerTag}", _consumerTag);

            await WaitForCancellationAsync(stoppingToken);
        }
        finally
        {
            await CleanupAsync();
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs args, string queueName, CancellationToken stoppingToken)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.Span);
            _logger.LogInformation("Received OrderConfirmedEvent with delivery tag {Tag}", args.DeliveryTag);
            using var scope = _serviceProvider.CreateScope();
            var envelope = DeserializeEnvelope(payload);
            if (envelope == null)
            {
                _logger.LogWarning("Invalid envelope received on queue {Queue}", queueName);
                throw new InvalidOperationException("Invalid event envelope");
            }

            var processedEventRepository = scope.ServiceProvider.GetRequiredService<IProcessedEventRepository>();
            var handler = scope.ServiceProvider.GetRequiredService<IOrderConfirmedHandler>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            if (await processedEventRepository.ExistsAsync(envelope.EventId, queueName, stoppingToken))
            {
                _logger.LogInformation("OrderConfirmed event {EventId} already processed", envelope.EventId);
                await AckAsync(args.DeliveryTag, stoppingToken);
                return;
            }

            var processed = ProcessedEvent.Create(envelope.EventType, queueName, envelope.EventId, envelope.OccurredOn);
            await processedEventRepository.AddAsync(processed, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            await handler.HandleAsync(envelope, stoppingToken);

            processed.MarkAsProcessed();
            await unitOfWork.SaveChangesAsync(stoppingToken);

            await AckAsync(args.DeliveryTag, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handling OrderConfirmed message with delivery tag {Tag}", args.DeliveryTag);
            await NackAsync(args.DeliveryTag, stoppingToken);
        }
    }

	private static Task WaitForCancellationAsync(CancellationToken stoppingToken)
		=> Task.Delay(Timeout.Infinite, stoppingToken);

    private async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up OrderConfirmed consumer");
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
			_logger.LogError(ex, "Error while cancelling consumer for {Queue}", _options.OrderConfirmedQueue);
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
			_logger.LogError(ex, "Error while closing channel for {Queue}", _options.OrderConfirmedQueue);
		}

        _channel.Dispose();
        _channel = null;
        _consumerTag = null;
        _logger.LogInformation("OrderConfirmed consumer cleanup completed");
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

    private async Task NackAsync(ulong deliveryTag, CancellationToken ct)
    {
        var channel = _channel;
        if (channel == null)
        {
            return;
        }

        await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true, cancellationToken: ct);
    }

    public override void Dispose()
    {
        CleanupAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
