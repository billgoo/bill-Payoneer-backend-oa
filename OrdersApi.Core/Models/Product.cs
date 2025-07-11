using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OrdersApi.Core.Models
{
    public class Product
    {
        [Required]
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public int Quantity { get; set; }
    }
}
