using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqConnectionProvider : IDisposable
{
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly IConnection _connection;

    public RabbitMqConnectionProvider(RabbitMqOptions options, ILogger<RabbitMqConnectionProvider> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public IConnection GetConnection() => _connection;

    public void Dispose()
    {
		if (_connection.IsOpen)
		{
			_logger.LogInformation("Closing RabbitMQ connection");
			try
			{
				_connection.CloseAsync().GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while closing RabbitMQ connection");
			}
		}

        _connection.Dispose();
    }
}
