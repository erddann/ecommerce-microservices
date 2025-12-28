using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Abstractions
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
