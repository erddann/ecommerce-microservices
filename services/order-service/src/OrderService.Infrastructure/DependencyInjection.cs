using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Infrastructure.BackgroundJobs;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Context;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Infrastructure.Settings;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool includeMessagingWorkers = true)
        {
            services.AddScoped<OrderService.Application.Abstractions.IOrderRepository, OrderRepository>();
            services.AddScoped<OrderService.Application.Abstractions.IProcessedEventRepository, ProcessedEventRepository>();
            services.AddScoped<OrderService.Application.Abstractions.IOutboxRepository, OutboxRepository>();
            services.AddScoped<OrderService.Application.Abstractions.IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(OrderService.Application.Abstractions.IGenericRepository<>), typeof(GenericRepository<>));

            services.AddSettings<MessagingSettings>(configuration, "Messaging");
            services.AddSettings<OutboxSettings>(configuration, "Outbox");

            services.AddDbContextPool<OrderDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("OrderDb"));
            });

            services.AddSingleton<RabbitMQ.Client.IConnection>(_ =>
            {
                var rabbitSection = configuration.GetSection("RabbitMq");
                var hostName = rabbitSection["HostName"] ?? "localhost";
                var portValue = rabbitSection["Port"];
                var port = !int.TryParse(portValue, out var p) ? 5672 : p;

                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = hostName,
                    Port = port
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            if (includeMessagingWorkers)
            {
                // Publisher only; consumers are registered in Worker project
                services.AddHostedService<OutboxPublisherWorker>();
            }

            // Event Bus
            services.AddSingleton<IEventBus, RabbitMqEventBus>();

            return services;
        }
    }
}
