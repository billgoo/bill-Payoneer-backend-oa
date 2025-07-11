using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrdersApi.Core.Models;
using OrdersApi.Core.Repositories;
using OrdersApi.Core.Services.Concrete;

namespace OrdersApi.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderService(null!, _mockLogger.Object));
    }

    [Fact]
    public async Task GetOrdersAsync_WithNoIds_ShouldReturnAllOrders()
    {
        // Arrange
        var expectedOrders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 1" },
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 2" }
        };

        _mockRepository.Setup(r => r.GetOrdersAsync(null))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _orderService.GetOrdersAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedOrders);
        _mockRepository.Verify(r => r.GetOrdersAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_WithSpecificIds_ShouldReturnFilteredOrders()
    {
        // Arrange
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var orderIds = new List<Guid> { orderId1, orderId2 };

        var expectedOrders = new List<Order>
        {
            new() { Id = orderId1, CustomerName = "Customer 1" },
            new() { Id = orderId2, CustomerName = "Customer 2" }
        };

        _mockRepository.Setup(r => r.GetOrdersAsync(orderIds))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _orderService.GetOrdersAsync(orderIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedOrders);
        _mockRepository.Verify(r => r.GetOrdersAsync(orderIds), Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var orderIds = new List<Guid>();
        var expectedOrders = new List<Order>();

        _mockRepository.Setup(r => r.GetOrdersAsync(orderIds))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _orderService.GetOrdersAsync(orderIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetOrdersAsync(orderIds), Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_WhenRepositoryThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new Exception("Database error");
        _mockRepository.Setup(r => r.GetOrdersAsync(It.IsAny<IEnumerable<Guid>?>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => _orderService.GetOrdersAsync(null));
        thrownException.Should().Be(exception);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while retrieving orders")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithValidOrders_ShouldReturnUpsertedOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 1" },
            new() { Id = Guid.NewGuid(), CustomerName = "Customer 2" }
        };

        _mockRepository.Setup(r => r.UpsertOrdersAsync(orders))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(orders);
        _mockRepository.Verify(r => r.UpsertOrdersAsync(orders), Times.Once);
    }

    [Fact]
    public async Task UpsertOrdersAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var orders = new List<Order>();

        _mockRepository.Setup(r => r.UpsertOrdersAsync(orders))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.UpsertOrdersAsync(orders);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.UpsertOrdersAsync(orders), Times.Once);
    }

    [Fact]
    public async Task UpsertOrdersAsync_WhenRepositoryThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var orders = new List<Order> { new() { CustomerName = "Test" } };
        var exception = new Exception("Database error");

        _mockRepository.Setup(r => r.UpsertOrdersAsync(orders))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => _orderService.UpsertOrdersAsync(orders));
        thrownException.Should().Be(exception);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while upserting orders")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
