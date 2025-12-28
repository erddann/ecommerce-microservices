using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Application;
using OrderService.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register infrastructure (only publisher hosted service comes from here)
        services.AddInfrastructure(context.Configuration, includeMessagingWorkers: true);

        // Application services (use-cases, MediatR, etc.)
        services.AddApplication();

        // Hosted consumers
        services.AddHostedService<OrderService.Worker.BackgroundJobs.OrderStockFailWorker>();
        services.AddHostedService<OrderService.Worker.BackgroundJobs.OrderStockCompletedWorker>();
    })
    .Build();

await host.RunAsync();
