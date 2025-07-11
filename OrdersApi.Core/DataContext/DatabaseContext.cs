using Microsoft.EntityFrameworkCore;
using OrdersApi.Core.Models;
using Newtonsoft.Json;

namespace OrdersApi.Core.DataContext
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Products as JSON column within Orders table
            modelBuilder.Entity<Order>()
                .Property(o => o.Items)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Product>>(v) ?? new List<Product>());

            // Seed Orders with embedded Products
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    CustomerName = "Test Customer 1",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Items = new List<Product>
                    {
                        new Product
                        {
                            ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                            Quantity = 1
                        }
                    }
                },
                new Order
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    CustomerName = "Test Customer 2",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Items = new List<Product>
                    {
                        new Product
                        {
                            ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                            Quantity = 100
                        },
                        new Product
                        {
                            ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                            Quantity = 0
                        }
                    }
                });
        }
    }
}
