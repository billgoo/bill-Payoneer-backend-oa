using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Core.Models
{
    public class Order
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [RegularExpression(@"^\S.*$", ErrorMessage = "CustomerName cannot be empty or whitespace and must start with a non-whitespace character.")]
        public required string CustomerName { get; set; }

        public List<Product> Items { get; set; } = new List<Product>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
