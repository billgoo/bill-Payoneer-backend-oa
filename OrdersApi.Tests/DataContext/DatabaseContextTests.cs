using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrdersApi.Core.DataContext;
using OrdersApi.Core.Models;

namespace OrdersApi.Tests.DataContext;

public class DatabaseContextTests : IDisposable
{
    private readonly DatabaseContext _context;

    public DatabaseContextTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DatabaseContext(options);
    }

    [Fact]
    public void DatabaseContext_ShouldHaveOrdersDbSet()
    {
        // Assert
        _context.Orders.Should().NotBeNull();
    }

    [Fact]
    public async Task DatabaseContext_CanAddAndRetrieveOrders()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Items = new List<Product>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 5 }
            }
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders.FirstOrDefaultAsync();
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.CustomerName.Should().Be("Test Customer");
        retrievedOrder.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task DatabaseContext_JsonSerializationOfItems_ShouldWorkCorrectly()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "JSON Test Customer",
            Items = new List<Product>
            {
                new() { ProductId = productId1, Quantity = 10 },
                new() { ProductId = productId2, Quantity = 20 }
            }
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Clear context to ensure fresh retrieval
        _context.ChangeTracker.Clear();

        var retrievedOrder = await _context.Orders.FirstOrDefaultAsync();

        // Assert
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Items.Should().HaveCount(2);
        retrievedOrder.Items.Should().Contain(p => p.ProductId == productId1 && p.Quantity == 10);
        retrievedOrder.Items.Should().Contain(p => p.ProductId == productId2 && p.Quantity == 20);
    }

    [Fact]
    public async Task DatabaseContext_CanUpdateOrderWithNewItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerName = "Update Test Customer",
            Items = new List<Product>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 5 }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act - Update with new items
        var retrievedOrder = await _context.Orders.FirstAsync();
        retrievedOrder.Items = new List<Product>
        {
            new() { ProductId = Guid.NewGuid(), Quantity = 15 },
            new() { ProductId = Guid.NewGuid(), Quantity = 25 }
        };

        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var updatedOrder = await _context.Orders.FirstAsync();
        updatedOrder.Items.Should().HaveCount(2);
        updatedOrder.Items.Should().Contain(p => p.Quantity == 15);
        updatedOrder.Items.Should().Contain(p => p.Quantity == 25);
    }

    [Fact]
    public async Task DatabaseContext_CanHandleEmptyItemsList()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Empty Items Customer",
            Items = new List<Product>()
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders.FirstOrDefaultAsync();
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Items.Should().NotBeNull();
        retrievedOrder.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DatabaseContext_MultipleOrders_ShouldMaintainSeparateItems()
    {
        // Arrange
        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Customer 1",
            Items = new List<Product>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Customer 2",
            Items = new List<Product>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 3 }
            }
        };

        // Act
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Assert
        var orders = await _context.Orders.ToListAsync();
        orders.Should().HaveCount(2);

        var retrievedOrder1 = orders.First(o => o.CustomerName == "Customer 1");
        var retrievedOrder2 = orders.First(o => o.CustomerName == "Customer 2");

        retrievedOrder1.Items.Should().HaveCount(1);
        retrievedOrder2.Items.Should().HaveCount(2);
    }

    [Fact]
    public void DatabaseContext_OnModelCreating_ShouldConfigureJsonConversion()
    {
        // This test verifies that the model configuration doesn't throw exceptions
        // Act & Assert
        var model = _context.Model;
        var orderEntityType = model.FindEntityType(typeof(Order));

        orderEntityType.Should().NotBeNull();
        var itemsProperty = orderEntityType!.FindProperty(nameof(Order.Items));
        itemsProperty.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
