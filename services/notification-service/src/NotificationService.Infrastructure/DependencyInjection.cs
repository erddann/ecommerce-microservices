using System;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool includeMessagingWorkers = false)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<NotificationSenderOptions>(configuration.GetSection(NotificationSenderOptions.SectionName));
        services.Configure<CustomerServiceOptions>(configuration.GetSection(CustomerServiceOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<NotificationSenderOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<CustomerServiceOptions>>().Value);

        var connectionString = configuration.GetConnectionString("NotificationDb") ?? string.Empty;
        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        if (includeMessagingWorkers)
        {
            services.AddSingleton<RabbitMqConnectionProvider>();
        }

        services.AddHttpClient<EmailSender>();
        services.AddHttpClient<SmsSender>();
		services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>((sp, client) =>
		{
			var options = sp.GetRequiredService<CustomerServiceOptions>();
			if (Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out var baseAddress))
			{
				client.BaseAddress = baseAddress;
			}
		})
		.AddPolicyHandler(GetRetryPolicy())
		.AddPolicyHandler(GetCircuitBreakerPolicy());
        services.AddScoped<IChannelSender, EmailSender>();
        services.AddScoped<IChannelSender, SmsSender>();
        services.AddScoped<INotificationSender, NotificationSender>();

        return services;
    }

	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		=> HttpPolicyExtensions
			.HandleTransientHttpError()
			.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

	private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
		=> HttpPolicyExtensions
			.HandleTransientHttpError()
			.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
