namespace StockService.Infrastructure.Configuration
{
    public interface ISettings<out TSettings>
        where TSettings : class
    {
        TSettings Value { get; }
    }

    internal sealed class SettingsWrapper<TSettings> : ISettings<TSettings>
        where TSettings : class
    {
        public SettingsWrapper(TSettings value)
        {
            Value = value;
        }

        public TSettings Value { get; }
    }
}
