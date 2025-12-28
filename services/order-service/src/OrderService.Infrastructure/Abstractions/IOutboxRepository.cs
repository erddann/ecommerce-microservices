using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Abstractions
{
    public interface IOutboxRepository
    {
        Task AddAsync(object domainEvent, CancellationToken cancellationToken = default);
    }
}
