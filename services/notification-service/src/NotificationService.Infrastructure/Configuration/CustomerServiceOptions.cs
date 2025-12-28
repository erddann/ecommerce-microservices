namespace NotificationService.Infrastructure.Configuration;

public class CustomerServiceOptions
{
    public const string SectionName = "CustomerService";
    public string BaseAddress { get; set; } = "https://customerservice";
}
