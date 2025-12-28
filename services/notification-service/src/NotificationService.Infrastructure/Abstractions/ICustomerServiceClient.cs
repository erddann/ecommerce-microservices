using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Abstractions;

public record CustomerProfile(Guid CustomerId, string FullName, string Email);

public interface ICustomerServiceClient
{
    Task<CustomerProfile> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken);
}
