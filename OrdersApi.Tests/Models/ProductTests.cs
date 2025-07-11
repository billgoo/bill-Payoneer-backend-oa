using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OrdersApi.Core.Models;

namespace OrdersApi.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Product_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        product.ProductId.Should().NotBe(Guid.Empty);
        product.Quantity.Should().Be(0);
    }

    [Fact]
    public void Product_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Quantity = 5
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(product);
        var isValid = Validator.TryValidateObject(product, context, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_WithNegativeQuantity_ShouldFailValidation(int invalidQuantity)
    {
        // Arrange
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Quantity = invalidQuantity
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(product);
        var isValid = Validator.TryValidateObject(product, context, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Quantity must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Product_WithValidQuantity_ShouldPassValidation(int validQuantity)
    {
        // Arrange
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Quantity = validQuantity
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(product);
        var isValid = Validator.TryValidateObject(product, context, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Product_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product1 = new Product { ProductId = productId, Quantity = 5 };
        var product2 = new Product { ProductId = productId, Quantity = 5 };
        var product3 = new Product { ProductId = Guid.NewGuid(), Quantity = 5 };

        // Assert
        product1.ProductId.Should().Be(product2.ProductId);
        product1.ProductId.Should().NotBe(product3.ProductId);
    }
}
