using OrdersApi.Core.Models;

namespace OrdersApi.Core.Services
{
    public interface IOrderService
    {
        Task<List<Order>> UpsertOrdersAsync(List<Order> orders);
        Task<List<Order>> GetOrdersAsync(List<Guid>? orderIds = null);
    }
}
