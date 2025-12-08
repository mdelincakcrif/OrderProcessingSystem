using Microsoft.EntityFrameworkCore;
using WebApi.Users.Domain;
using WebApi.Products.Domain;
using WebApi.Orders.Domain;
using WebApi.Notifications.Domain;

namespace WebApi.Dal;

public class OrderProcessingDbContext : DbContext
{
    public OrderProcessingDbContext(DbContextOptions<OrderProcessingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderProcessingDbContext).Assembly);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed users
        var user1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var user2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminUserId,
                Name = "Admin",
                Email = "admin@example.com",
                // Pre-computed BCrypt hash for "demo123$!" - using cost factor 11
                // Generated with: BCrypt.Net.BCrypt.HashPassword("demo123$!", 11)
                Password = "$2a$11$gR.MbESvO35KWLXJOZnWtuvs6fz4l1XE7bjj3u4NgiaqYiBZb8Xdy"
            },
            new User
            {
                Id = user1Id,
                Name = "John Doe",
                Email = "john@example.com",
                // Pre-computed BCrypt hash for "password123" - using cost factor 11
                // Generated with: BCrypt.Net.BCrypt.HashPassword("password123", 11)
                Password = "$2a$11$YGQ4M7F8z7t9UwQXxYzVxO4cOE9IzXRz9HYN.R8zG8XwZ7T9QwZ8u"
            },
            new User
            {
                Id = user2Id,
                Name = "Jane Smith",
                Email = "jane@example.com",
                // Pre-computed BCrypt hash for "password123" - using cost factor 11
                Password = "$2a$11$YGQ4M7F8z7t9UwQXxYzVxO4cOE9IzXRz9HYN.R8zG8XwZ7T9QwZ8u"
            }
        );

        // Seed products
        var product1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var product2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var product3Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = product1Id,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 10,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = product2Id,
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = product3Id,
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 79.99m,
                Stock = 30,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
