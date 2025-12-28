using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockService.Application.Contracts;
using StockService.Application.Handlers;
using StockService.Infrastructure;
using StockService.Infrastructure.Configuration;
using StockService.Worker.BackgroundJobs;
using ApplicationStockService = StockService.Application.Services.StockService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure(
            context.Configuration,
            includeMessagingWorkers: true);

        services.AddSettings<MessagingSettings>(context.Configuration, "Messaging");
        services.AddSettings<NotificationSettings>(context.Configuration, "Notification");

        services.AddScoped<IStockService, ApplicationStockService>();
        services.AddScoped<IOrderCreatedMessageHandler, OrderCreatedMessageHandler>();
        services.AddHostedService<OrderCreatedConsumerWorker>();
    })
    .Build();

await host.RunAsync();
