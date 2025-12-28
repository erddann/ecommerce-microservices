using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Configuration;

namespace NotificationService.Infrastructure.Services;

public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerServiceClient> _logger;

    public CustomerServiceClient(HttpClient httpClient, ILogger<CustomerServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CustomerProfile> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/customers/{customerId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerProfile>(cancellationToken: cancellationToken);
                if (customer != null)
                {
                    return EnsureDefaults(customer);
                }
            }
            else
            {
                _logger.LogWarning("CustomerService responded {StatusCode} for {CustomerId}", response.StatusCode, customerId);
            }
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "CustomerService request failed for {CustomerId}", customerId);
		}

        return BuildMockCustomer(customerId);
    }

    private static CustomerProfile BuildMockCustomer(Guid customerId)
    {
        var shortId = customerId.ToString("N")[..8];
        return new CustomerProfile(customerId, $"Customer {shortId}", $"customer{shortId}@example.com");
    }

    private static CustomerProfile EnsureDefaults(CustomerProfile profile)
    {
        var fullName = string.IsNullOrWhiteSpace(profile.FullName) ? $"Customer {profile.CustomerId.ToString("N")[..8]}" : profile.FullName;
        var email = string.IsNullOrWhiteSpace(profile.Email) ? $"customer{profile.CustomerId.ToString("N")[..8]}@example.com" : profile.Email;
        return profile with { FullName = fullName, Email = email };
    }
}
