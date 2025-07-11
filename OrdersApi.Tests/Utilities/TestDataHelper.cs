using OrdersApi.Core.Models;

namespace OrdersApi.Tests.Utilities;

public static class TestDataHelper
{
    public static Order CreateValidOrder(string? customerName = null, Guid? id = null)
    {
        return new Order
        {
            Id = id ?? Guid.NewGuid(),
            CustomerName = customerName ?? "Test Customer",
            Items = new List<Product>()
        };
    }

    public static Product CreateValidProduct(Guid? productId = null, int quantity = 1)
    {
        return new Product
        {
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity
        };
    }

    public static Order CreateOrderWithItems(string customerName = "Test Customer", int itemCount = 2)
    {
        var order = CreateValidOrder(customerName);

        for (int i = 0; i < itemCount; i++)
        {
            order.Items.Add(CreateValidProduct(quantity: i + 1));
        }

        return order;
    }

    public static List<Order> CreateOrderList(int count = 3)
    {
        var orders = new List<Order>();

        for (int i = 0; i < count; i++)
        {
            orders.Add(CreateOrderWithItems($"Customer {i + 1}", i + 1));
        }

        return orders;
    }

    public static Order CreateInvalidOrder()
    {
        return new Order
        {
            Id = Guid.Empty,
            CustomerName = " ", // Invalid: whitespace only
            Items = new List<Product>()
        };
    }

    public static Product CreateInvalidProduct()
    {
        return new Product
        {
            ProductId = Guid.Empty, // Invalid
            Quantity = -1 // Invalid
        };
    }
}
