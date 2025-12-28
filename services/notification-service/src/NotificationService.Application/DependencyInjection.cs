using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Handlers;
using NotificationService.Application.Services;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TemplateBinder>();
        services.AddScoped<NotificationContextBuilder>();
		services.AddScoped<IOrderNotificationService, OrderNotificationService>();
        services.AddScoped<IOrderCancelledHandler, OrderCancelledHandler>();
        services.AddScoped<IOrderConfirmedHandler, OrderConfirmedHandler>();
        return services;
    }
}
