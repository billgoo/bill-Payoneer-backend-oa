using Microsoft;
using Microsoft.Extensions.Logging;
using OrdersApi.Core.Models;
using OrdersApi.Core.Repositories;

namespace OrdersApi.Core.Services.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService>? _logger;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            Requires.NotNull(orderRepository, nameof(orderRepository));
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<List<Order>> GetOrdersAsync(List<Guid>? orderIds = null)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersAsync(orderIds);
                return orders.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while retrieving orders.");
                throw;
            }
        }

        public async Task<List<Order>> UpsertOrdersAsync(List<Order> orders)
        {
            try
            {
                return (await _orderRepository.UpsertOrdersAsync(orders)).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while upserting orders.");
                throw;
            }
        }
    }
}
