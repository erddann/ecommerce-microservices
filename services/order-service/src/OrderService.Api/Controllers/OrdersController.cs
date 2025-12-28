using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Contracts;
using OrderService.Application.Contracts;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            var orderId = await _orderService.CreateOrderAsync(
                request.CustomerId,
                request.Items.Select(i => (i.ProductId, i.Quantity)),
                saveChanges: true,
                cancellationToken: HttpContext.RequestAborted);

            return CreatedAtAction(nameof(Create), new { id = orderId }, null);
        }
    }
}
