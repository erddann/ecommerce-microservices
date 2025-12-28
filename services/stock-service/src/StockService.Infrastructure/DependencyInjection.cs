using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockService.Infrastructure.BackgroundJobs;
using StockService.Infrastructure.Messaging;
using StockService.Infrastructure.Persistence.Context;
using StockService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockService.Infrastructure.Abstractions;
using StockService.Infrastructure.Configuration;

namespace StockService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool includeMessagingWorkers = true)
        {
            if (includeMessagingWorkers)
            {
                var rabbitHost = configuration["RabbitMq:HostName"] ?? "rabbitmq";
                var rabbitPortString = configuration["RabbitMq:Port"];
                var rabbitPort = int.TryParse(rabbitPortString, out var portValue)
                    ? portValue
                    : 5672;

                services.AddSingleton<RabbitMQ.Client.IConnection>(_ =>
                {
                    var factory = new RabbitMQ.Client.ConnectionFactory
                    {
                        HostName = rabbitHost,
                        Port = rabbitPort
                    };

                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                });

                services.AddSingleton<IEventBus, RabbitMqEventBus>();
                services.AddSettings<OutboxPublisherSettings>(configuration, "OutboxPublisher");

                services.AddHostedService<OutboxPublisherWorker>();
            }

            services.AddDbContext<StockDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("StockDb"));
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
          
            return services;
        }
    }
}
