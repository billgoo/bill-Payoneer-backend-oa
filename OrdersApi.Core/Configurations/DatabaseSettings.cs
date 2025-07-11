using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Core.Configurations
{
    public class DatabaseSettings
    {
        [Required]
        [RegularExpression(@"\S+", ErrorMessage = "ConnectionString cannot be empty or whitespace.")]
        public required string ConnectionString { get; set; } = string.Empty;
    }
}
