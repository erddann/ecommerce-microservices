using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure.Configuration;

public interface ISettings<out T>
    where T : class
{
    T Value { get; }
}

public static class SettingsExtensions
{
    public static IServiceCollection AddSettings<TSettings>(this IServiceCollection services, IConfiguration configuration, string sectionName)
        where TSettings : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(sectionName);

        services.Configure<TSettings>(configuration.GetSection(sectionName));
        services.AddSingleton<ISettings<TSettings>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TSettings>>();
            return new SettingsWrapper<TSettings>(options.Value);
        });

        return services;
    }

    private sealed class SettingsWrapper<TSettings> : ISettings<TSettings>
        where TSettings : class
    {
        public SettingsWrapper(TSettings value)
        {
            Value = value;
        }

        public TSettings Value { get; }
    }
}
