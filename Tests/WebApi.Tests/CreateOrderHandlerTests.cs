using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using WebApi.Orders.Domain;
using WebApi.Orders.DTOs;
using WebApi.Orders.Features.CreateOrder;
using WebApi.Products.Domain;
using WebApi.Tests.TestHelpers;
using WebApi.Users.Domain;
using Xunit;

namespace WebApi.Tests;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_CreatesOrder()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        // Create test user and product
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "testuser@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);

        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test",
            Price = 50.00m,
            Stock = 100,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new CreateOrderHandler(context, publishEndpoint);
        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemRequest> { new CreateOrderItemRequest(productId, 2, 50.00m) }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.UserId == userId);
        order.Should().NotBeNull();
        order!.Total.Should().Be(100.00m);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().HaveCount(1);
        order.Items[0].ProductId.Should().Be(productId);
        order.Items[0].Quantity.Should().Be(2);
        order.Items[0].Price.Should().Be(50.00m);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_CalculatesCorrectTotal()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "testuser@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);

        var product1Id = Guid.NewGuid();
        var product1 = new Product
        {
            Id = product1Id,
            Name = "Product 1",
            Description = "Test",
            Price = 10.00m,
            Stock = 100,
            CreatedAt = DateTime.UtcNow
        };
        var product2Id = Guid.NewGuid();
        var product2 = new Product
        {
            Id = product2Id,
            Name = "Product 2",
            Description = "Test",
            Price = 25.00m,
            Stock = 100,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new CreateOrderHandler(context, publishEndpoint);
        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(product1Id, 3, 10.00m), // 30.00
                new CreateOrderItemRequest(product2Id, 2, 25.00m)  // 50.00
            }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.UserId == userId);
        order.Should().NotBeNull();
        order!.Total.Should().Be(80.00m); // 30 + 50
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithInvalidProduct_ReturnsBadRequest()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "testuser@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new CreateOrderHandler(context, publishEndpoint);
        var nonExistentProductId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemRequest> { new CreateOrderItemRequest(nonExistentProductId, 1, 10.00m) }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var orderCount = await context.Orders.CountAsync();
        orderCount.Should().Be(0);
    }
}
