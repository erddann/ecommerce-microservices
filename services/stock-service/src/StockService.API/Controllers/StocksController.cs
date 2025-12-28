using Microsoft.AspNetCore.Mvc;
using StockService.Application.Contracts;
using StockService.Infrastructure.Contracts.Stocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StockService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockDto>>> GetStocks(CancellationToken cancellationToken)
        {
            var stocks = await _stockService.GetStocksAsync(cancellationToken);
            return Ok(stocks);
        }

        [HttpGet("{productId:guid}")]
        public async Task<ActionResult<StockDto>> GetStock(Guid productId, CancellationToken cancellationToken)
        {
            var stock = await _stockService.GetStockAsync(productId, cancellationToken);
            if (stock == null)
            {
                return NotFound();
            }

            return Ok(stock);
        }

        [HttpPost]
        public async Task<ActionResult<StockDto>> CreateStock(
            [FromBody] CreateStockRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ProductId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(request.ProductId), "ProductId must be provided.");
                return ValidationProblem(ModelState);
            }

            var stock = await _stockService.CreateStockAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetStock), new { productId = stock.ProductId }, stock);
        }

        [HttpPut("{productId:guid}")]
        public async Task<IActionResult> UpdateStock(
            Guid productId,
            [FromBody] UpdateStockRequest request,
            CancellationToken cancellationToken)
        {
            var updated = await _stockService.UpdateStockAsync(productId, request, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> DeleteStock(Guid productId, CancellationToken cancellationToken)
        {
            var deleted = await _stockService.DeleteStockAsync(productId, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
