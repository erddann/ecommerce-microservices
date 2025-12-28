using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Messaging
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly IConnection _connection;

        public RabbitMqEventBus(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync(
            string exchangeName,
            string routingKey,
            string payload,
            CancellationToken ct)
        {
            var channel = await _connection.CreateChannelAsync(null, ct);

            try
            {
                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    cancellationToken: ct);
              
                var body = Encoding.UTF8.GetBytes(payload);

                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    body: body,
                    cancellationToken: ct);
            }
            finally
            {
                await channel.CloseAsync(ct);
            }
        }
    }
}
