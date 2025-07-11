using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrdersApi.API.Controller;
using OrdersApi.Core.Models;
using OrdersApi.Core.Services;

namespace OrdersApi.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullOrderService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrdersController(null!, _mockLogger.Object));
    }

    [Fact]
    public async Task GetOrders_WithNoIds_ShouldReturnAllOrders()
    {
        // Arrange
        var expectedOrders = new List<Order>
        {
            new() { CustomerName = "Customer 1" },
            new() { CustomerName = "Customer 2" }
        };

        _mockOrderService.Setup(s => s.GetOrdersAsync(null))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedOrders);

        _mockOrderService.Verify(s => s.GetOrdersAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetOrders_WithSpecificIds_ShouldReturnFilteredOrders()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var expectedOrders = new List<Order>
        {
            new() { Id = orderIds[0], CustomerName = "Customer 1" },
            new() { Id = orderIds[1], CustomerName = "Customer 2" }
        };

        _mockOrderService.Setup(s => s.GetOrdersAsync(orderIds))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _controller.GetOrders(orderIds);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedOrders);

        _mockOrderService.Verify(s => s.GetOrdersAsync(orderIds), Times.Once);
    }

    [Fact]
    public async Task GetOrders_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var exception = new Exception("Service error");
        _mockOrderService.Setup(s => s.GetOrdersAsync(It.IsAny<List<Guid>?>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();

        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Internal server error");

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving orders")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithValidOrders_ShouldReturnCreatedResult()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { CustomerName = "Customer 1" },
            new() { CustomerName = "Customer 2" }
        };

        _mockOrderService.Setup(s => s.UpsertOrdersAsync(orders))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.CreateOrder(orders);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(OrdersController.GetOrders));
        createdResult.Value.Should().BeEquivalentTo(orders);

        _mockOrderService.Verify(s => s.UpsertOrdersAsync(orders), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { CustomerName = "Valid Customer" }
        };

        var exception = new Exception("Service error");
        _mockOrderService.Setup(s => s.UpsertOrdersAsync(orders))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.CreateOrder(orders);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();

        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Internal server error");

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating order")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
