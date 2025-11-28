// File: Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ShopApi.Models;         // <- important

namespace ShopApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique index on user email
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Order -> OrderItems (1-to-many) cascade delete
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product (many-to-one), prevent deleting products referenced by order items
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Example seed data (optional)
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Mechanical Keyboard", Description = "Tactile keyboard", Price = 59.99m },
                new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic mouse", Price = 29.99m }
            );

            builder.Entity<User>().HasData(
                new User { Id = 1, FullName = "Demo User", Email = "demo@example.com" }
            );
        }
    }
}
