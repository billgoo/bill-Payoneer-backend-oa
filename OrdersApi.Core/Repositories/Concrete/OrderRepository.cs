using Microsoft;
using Microsoft.EntityFrameworkCore;
using OrdersApi.Core.DataContext;
using OrdersApi.Core.Models;

namespace OrdersApi.Core.Repositories.Concrete
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DatabaseContext _databaseContext;

        public OrderRepository(DatabaseContext databaseContext)
        {
            Requires.NotNull(databaseContext, nameof(databaseContext));
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(IEnumerable<Guid>? orderIds = null)
        {
            if (orderIds == null)
            {
                return await _databaseContext.Orders
                    .AsNoTracking()
                    .ToListAsync();
            }
            return await _databaseContext.Orders
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> UpsertOrdersAsync(IEnumerable<Order> orders)
        {
            var orderIds = orders.Select(o => o.Id).ToList();
            var currOrders = await GetOrdersAsync(orderIds);

            BatchCreateOrUpdateOrders(currOrders, orders);

            await _databaseContext.SaveChangesAsync();
            return orders;
        }

        private void BatchCreateOrUpdateOrders(IEnumerable<Order> existingOrders, IEnumerable<Order> newOrders)
        {
            foreach (var order in newOrders)
            {
                var existingOrder = existingOrders.FirstOrDefault(o => o.Id == order.Id);
                if (existingOrder != null)
                {
                    _databaseContext.Entry(existingOrder).CurrentValues.SetValues(order);
                }
                else
                {
                    _databaseContext.Orders.Add(order);
                }
            }
        }
    }
}
