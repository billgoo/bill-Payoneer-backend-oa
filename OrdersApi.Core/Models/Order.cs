using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Core.Models
{
    public class Order
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required string CustomerName { get; set; }

        public List<Product> Items { get; set; } = new List<Product>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
