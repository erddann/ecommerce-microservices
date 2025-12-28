using MediatR;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.CreateOrder
{
    public class CreateOrderCommandHandler: IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderService _orderService;

        public CreateOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var items = command.Items
                .Select(i => (i.ProductId, i.Quantity));

            return await _orderService.CreateOrderAsync(
                command.CustomerId,
                items,
                saveChanges: false,
                cancellationToken);
        }
    }
}
