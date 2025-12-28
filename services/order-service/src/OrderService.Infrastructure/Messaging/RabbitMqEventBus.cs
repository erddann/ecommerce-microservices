using RabbitMQ.Client;
using System.Text;
using OrderService.Infrastructure.Settings;

namespace OrderService.Infrastructure.Messaging
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly MessagingSettings _settings;

        public RabbitMqEventBus(IConnection connection, ISettings<MessagingSettings> settings)
        {
            _connection = connection;
            _settings = settings.Value;
        }

        public async Task PublishAsync(
            string eventType,
            string payload,
            CancellationToken ct)
        {
            var channel = await _connection.CreateChannelAsync(null, ct);

            var exchangeName = _settings.OutgoingExchange;

            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: ct);

            var body = Encoding.UTF8.GetBytes(payload);

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: eventType,
                body: body,
                cancellationToken: ct);

            await channel.CloseAsync(ct);
        }
    }
}
