using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrdersApi.Core.DataContext;
using OrdersApi.Core.Models;

namespace OrdersApi.Tests.Integration;

public class OrdersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrdersApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the existing database context registration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                // Remove all DbContextOptions<DatabaseContext> (generic) registrations
                var dbContextOptionsDescriptors = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.Contains("DbContextOptions")).ToList();
                foreach (var desc in dbContextOptionsDescriptors)
                {
                    services.Remove(desc);
                }
                // Add in-memory database for testing
                services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_WithNoOrders_ShouldReturnEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonConvert.DeserializeObject<List<Order>>(content);
        orders.Should().NotBeNull();
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task PostOrders_WithValidOrders_ShouldCreateOrdersSuccessfully()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerName = "Integration Test Customer 1",
                Items = new List<Product>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5 }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                CustomerName = "Integration Test Customer 2",
                Items = new List<Product>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 10 },
                    new() { ProductId = Guid.NewGuid(), Quantity = 15 }
                }
            }
        };

        var json = JsonConvert.SerializeObject(orders);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdOrders = JsonConvert.DeserializeObject<List<Order>>(responseContent);
        createdOrders.Should().NotBeNull();
        createdOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task PostOrders_WithInvalidOrders_ShouldReturnBadRequest()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerName = "", // Invalid: empty customer name
                Items = new List<Product>()
            }
        };

        var json = JsonConvert.SerializeObject(orders);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostOrders_WithEmptyList_ShouldReturnBadRequest()
    {
        // Arrange
        var orders = new List<Order>();
        var json = JsonConvert.SerializeObject(orders);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostOrders_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
