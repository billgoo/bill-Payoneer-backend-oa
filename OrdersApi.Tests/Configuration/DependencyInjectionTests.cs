using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OrdersApi.Core.Services;
using OrdersApi.Core.Repositories;
using OrdersApi.Core.DataContext;
using Microsoft.EntityFrameworkCore;

namespace OrdersApi.Tests.Configuration;

public class DependencyInjectionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DependencyInjectionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public void ServiceCollection_ShouldResolveOrderService()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var orderService = serviceProvider.GetService<IOrderService>();

        // Assert
        orderService.Should().NotBeNull();
    }

    [Fact]
    public void ServiceCollection_ShouldResolveOrderRepository()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var orderRepository = serviceProvider.GetService<IOrderRepository>();

        // Assert
        orderRepository.Should().NotBeNull();
    }

    [Fact]
    public void ServiceCollection_ShouldResolveDatabaseContext()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var databaseContext = serviceProvider.GetService<DatabaseContext>();

        // Assert
        databaseContext.Should().NotBeNull();
    }

    [Fact]
    public void ServiceCollection_ShouldResolveDbContextOptions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var dbContextOptions = serviceProvider.GetService<DbContextOptions<DatabaseContext>>();

        // Assert
        dbContextOptions.Should().NotBeNull();
    }

    [Fact]
    public void ServiceCollection_ServicesShouldHaveCorrectLifetime()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        // Act
        var service1 = scope1.ServiceProvider.GetService<IOrderService>();
        var service2 = scope2.ServiceProvider.GetService<IOrderService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBeSameAs(service2); // Should be scoped/transient, not singleton
    }
}
