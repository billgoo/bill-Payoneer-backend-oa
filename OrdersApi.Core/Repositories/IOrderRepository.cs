using OrdersApi.Core.Models;

namespace OrdersApi.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> UpsertOrdersAsync(IEnumerable<Order> orders);
        Task<IEnumerable<Order>> GetOrdersAsync(IEnumerable<Guid>? orderIds = null);
    }
}
