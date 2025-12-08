using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Dal;
using Worker.Jobs;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;
using Worker.Tests.TestHelpers;
using Xunit;

namespace Worker.Tests;

public class OrderExpirationJobTests
{
    [Fact]
    public async Task ProcessExpiredOrders_ExpiresOldProcessingOrders()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();

        // Create old order (>10 minutes in Processing)
        var oldOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
            Items = new List<OrderItem>()
        };

        // Create recent order (should not expire)
        var recentOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 50m,
            Status = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            Items = new List<OrderItem>()
        };

        context.Orders.AddRange(oldOrder, recentOrder);
        await context.SaveChangesAsync();

        // Create service scope factory mock
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(context);
        serviceCollection.AddSingleton(publishEndpoint);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var scope = serviceProvider.CreateScope();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<OrderExpirationJob>>();
        var job = new OrderExpirationJob(scopeFactory, logger);

        // Use reflection to call private method ProcessExpiredOrders
        var method = typeof(OrderExpirationJob).GetMethod("ProcessExpiredOrders",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method!.Invoke(job, new object[] { CancellationToken.None })!;

        // Assert
        var expiredOrder = await context.Orders.FindAsync(oldOrder.Id);
        expiredOrder.Should().NotBeNull();
        expiredOrder!.Status.Should().Be(OrderStatus.Expired);

        var stillProcessingOrder = await context.Orders.FindAsync(recentOrder.Id);
        stillProcessingOrder.Should().NotBeNull();
        stillProcessingOrder!.Status.Should().Be(OrderStatus.Processing);

        // Verify OrderExpiredEvent was published for expired order
        await publishEndpoint.Received(1).Publish(
            Arg.Is<OrderExpiredEvent>(e => e.OrderId == oldOrder.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessExpiredOrders_IgnoresCompletedOrders()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();

        // Create old completed order (should not be expired)
        var completedOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
            Items = new List<OrderItem>()
        };

        context.Orders.Add(completedOrder);
        await context.SaveChangesAsync();

        // Create service scope factory mock
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(context);
        serviceCollection.AddSingleton(publishEndpoint);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var scope = serviceProvider.CreateScope();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<OrderExpirationJob>>();
        var job = new OrderExpirationJob(scopeFactory, logger);

        // Use reflection to call private method ProcessExpiredOrders
        var method = typeof(OrderExpirationJob).GetMethod("ProcessExpiredOrders",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method!.Invoke(job, new object[] { CancellationToken.None })!;

        // Assert
        var order = await context.Orders.FindAsync(completedOrder.Id);
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Completed); // Should remain Completed

        // Verify no events were published
        await publishEndpoint.DidNotReceive().Publish(
            Arg.Any<OrderExpiredEvent>(),
            Arg.Any<CancellationToken>());
    }
}
