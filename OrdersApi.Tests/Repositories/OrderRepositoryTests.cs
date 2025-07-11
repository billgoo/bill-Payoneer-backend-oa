using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrdersApi.Core.DataContext;
using OrdersApi.Core.Models;
using OrdersApi.Core.Repositories.Concrete;

namespace OrdersApi.Tests.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DatabaseContext(options);
        _repository = new OrderRepository(_context);
    }

    [Fact]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderRepository(null!));
    }

    [Fact]
    public async Task GetOrdersAsync_WithNoOrders_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrdersAsync_WithNoIds_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 1", Items = new List<Product>() },
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 2", Items = new List<Product>() }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(o => o.CustomerName).Should().Contain(new[] { "Customer 1", "Customer 2" });
    }

    [Fact]
    public async Task GetOrdersAsync_WithSpecificIds_ShouldReturnFilteredOrders()
    {
        // Arrange
        var order1 = new Order { Id = Guid.NewGuid(), CustomerName = "Customer 1", Items = new List<Product>() };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerName = "Customer 2", Items = new List<Product>() };
        var order3 = new Order { Id = Guid.NewGuid(), CustomerName = "Customer 3", Items = new List<Product>() };

        _context.Orders.AddRange(order1, order2, order3);
        await _context.SaveChangesAsync();

        var orderIds = new List<Guid> { order1.Id, order3.Id };

        // Act
        var result = await _repository.GetOrdersAsync(orderIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(o => o.Id).Should().Contain(new[] { order1.Id, order3.Id });
        result.Select(o => o.Id).Should().NotContain(order2.Id);
    }

    [Fact]
    public async Task GetOrdersAsync_WithNonExistentIds_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await _repository.GetOrdersAsync(nonExistentIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithNewOrders_ShouldAddOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), CustomerName = "New Customer 1", Items = new List<Product>() },
            new() { Id = Guid.NewGuid(), CustomerName = "New Customer 2", Items = new List<Product>() }
        };

        // Act
        var result = await _repository.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var ordersInDb = await _context.Orders.ToListAsync();
        ordersInDb.Should().HaveCount(2);
        ordersInDb.Select(o => o.CustomerName).Should().Contain("New Customer 1", "New Customer 2");
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithExistingOrders_ShouldUpdateOrders()
    {
        // Arrange
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Original Customer",
            Items = new List<Product>()
        };

        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        // Modify the order
        var updatedOrder = new Order
        {
            Id = existingOrder.Id,
            CustomerName = "Updated Customer",
            Items = new List<Product>()
        };

        // Act
        var result = await _repository.UpsertOrdersAsync(new List<Order> { updatedOrder });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var orderInDb = await _context.Orders.FindAsync(existingOrder.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.CustomerName.Should().Be("Updated Customer");
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithMixedNewAndExistingOrders_ShouldHandleBoth()
    {
        // Arrange
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Existing Customer",
            Items = new List<Product>()
        };

        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        var orders = new List<Order>
        {
            new() { Id = existingOrder.Id, CustomerName = "Updated Existing Customer", Items = new List<Product>() },
            new() { Id = Guid.NewGuid(), CustomerName = "New Customer", Items = new List<Product>() }
        };

        // Act
        var result = await _repository.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var ordersInDb = await _context.Orders.ToListAsync();
        ordersInDb.Should().HaveCount(2);
        ordersInDb.Select(o => o.CustomerName).Should().Contain(new[] { "Updated Existing Customer", "New Customer" });
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var orders = new List<Order>();

        // Act
        var result = await _repository.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        var ordersInDb = await _context.Orders.ToListAsync();
        ordersInDb.Should().BeEmpty();
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithOrdersContainingItems_ShouldPreserveItems()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerName = "Customer with Items",
                Items = new List<Product>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5 },
                    new() { ProductId = Guid.NewGuid(), Quantity = 10 }
                }
            }
        };

        // Act
        var result = await _repository.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Items.Should().HaveCount(2);

        var orderInDb = await _context.Orders.FirstAsync();
        orderInDb.Items.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
