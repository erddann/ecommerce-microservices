using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.CreateOrder
{
    public record CreateOrderItemDto
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
