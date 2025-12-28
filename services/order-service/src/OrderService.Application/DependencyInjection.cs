using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.Behaviors;
using OrderService.Application.Contracts;
using OrderService.Application.Handlers;
using ApplicationOrderService = OrderService.Application.Services.OrderService;
using System.Reflection;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(CreateOrderCommandHandler).Assembly);
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddScoped<IOrderService, ApplicationOrderService>();
        services.AddScoped<IOrderStockCompletedMessageHandler, OrderStockCompletedMessageHandler>();
        services.AddScoped<IOrderStockFailMessageHandler, OrderStockFailMessageHandler>();

        return services;
    }
}
