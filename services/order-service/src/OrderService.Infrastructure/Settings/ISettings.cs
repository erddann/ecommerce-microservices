using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderService.Infrastructure.Settings
{
    public interface ISettings<out T>
        where T : class
    {
        T Value { get; }
    }

    internal sealed class SettingsWrapper<T> : ISettings<T>
        where T : class
    {
        public SettingsWrapper(IOptions<T> options)
        {
            Value = options.Value;
        }

        public T Value { get; }
    }

    public static class SettingsServiceCollectionExtensions
    {
        public static IServiceCollection AddSettings<TSettings>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where TSettings : class, new()
        {
            services.Configure<TSettings>(configuration.GetSection(sectionName));
            services.AddSingleton<ISettings<TSettings>, SettingsWrapper<TSettings>>();
            return services;
        }
    }
}
