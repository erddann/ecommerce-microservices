using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.CreateOrder
{
    public record CreateOrderCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; }
        public IReadOnlyCollection<(Guid ProductId, int Quantity)> Items { get; }

        public CreateOrderCommand(
            Guid customerId,
            IEnumerable<(Guid ProductId, int Quantity)> items)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("CustomerId cannot be empty.");

            var itemList = items?.ToList()
                ?? throw new ArgumentNullException(nameof(items));

            if (!itemList.Any())
                throw new ArgumentException("Order must contain at least one item.");

            CustomerId = customerId;
            Items = itemList;
        }
    }
}
