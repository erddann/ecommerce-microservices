using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StockService.Infrastructure.Configuration
{
    public static class ServiceCollectionSettingsExtensions
    {
        public static IServiceCollection AddSettings<TSettings>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where TSettings : class, new()
        {
            var settings = new TSettings();
            configuration.GetSection(sectionName).Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<ISettings<TSettings>>(new SettingsWrapper<TSettings>(settings));

            return services;
        }
    }
}
