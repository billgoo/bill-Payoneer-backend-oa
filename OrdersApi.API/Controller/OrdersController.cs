using Microsoft;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Core.Models;
using OrdersApi.Core.Services;

namespace OrdersApi.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController>? _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            Requires.NotNull(orderService, nameof(orderService));

            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] List<Guid>? orderIds = null)
        {
            try
            {
                _logger?.LogInformation("Getting orders with specified IDs");
                var orders = await _orderService.GetOrdersAsync(orderIds);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving orders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<Order>>> CreateOrder([FromBody] List<Order> orders)
        {
            if (orders == null)
            {
                return BadRequest("Request body is missing or invalid.");
            }
            if (orders.Count == 0)
            {
                return BadRequest("Order list cannot be empty.");
            }

            try
            {
                _logger?.LogInformation("Creating new {count} orders", orders.Count);
                var createdOrders = await _orderService.UpsertOrdersAsync(orders);

                return CreatedAtAction(
                    nameof(GetOrders),
                    new { orderIds = createdOrders.Select(o => o.Id) },
                    createdOrders);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
