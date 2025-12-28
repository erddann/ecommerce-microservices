using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Abstractions
{
    public interface IOutboxRepository
    {
        Task AddAsync(object domainEvent, CancellationToken cancellationToken);
    }
}
