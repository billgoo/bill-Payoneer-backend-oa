using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OrdersApi.Core.Models;

namespace OrdersApi.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Order_WithValidConstructor_ShouldSetDefaultValues()
    {
        // Act
        var order = new Order { CustomerName = "Test Customer" };

        // Assert
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerName.Should().Be("Test Customer");
        order.Items.Should().NotBeNull().And.BeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Order_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Items = new List<Product>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(order);
        var isValid = Validator.TryValidateObject(order, context, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Order_WithInvalidCustomerName_ShouldFailValidation(string invalidCustomerName)
    {
        // Arrange
        var order = new Order
        {
            CustomerName = invalidCustomerName
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(order);
        var isValid = Validator.TryValidateObject(order, context, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("The CustomerName field is required.");
    }

    [Fact]
    public void Order_CanAddMultipleItems()
    {
        // Arrange
        var order = new Order { CustomerName = "Test Customer" };
        var product1 = new Product { ProductId = Guid.NewGuid(), Quantity = 1 };
        var product2 = new Product { ProductId = Guid.NewGuid(), Quantity = 2 };

        // Act
        order.Items.Add(product1);
        order.Items.Add(product2);

        // Assert
        order.Items.Should().HaveCount(2);
        order.Items.Should().Contain(product1);
        order.Items.Should().Contain(product2);
    }
}
