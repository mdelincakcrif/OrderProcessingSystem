using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Orders.Domain;
using WebApi.Orders.EventConsumers;
using WebApi.Orders.Events;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class OrderCreatedConsumerTests
{
    [Fact]
    public async Task Consume_ChangesOrderStatusFromPending()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        // Create test order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.Total,
            order.CreatedAt
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.Should().NotBeNull();
        // Order should no longer be Pending (either Processing or Completed due to 50% chance)
        updatedOrder!.Status.Should().NotBe(OrderStatus.Pending);
        updatedOrder.Status.Should().BeOneOf(OrderStatus.Processing, OrderStatus.Completed);
    }

    [Fact]
    public async Task Consume_OrderStatusChangesAfterProcessing()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.Total,
            order.CreatedAt
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.Should().NotBeNull();

        // Order should be either Processing or Completed (50% chance)
        updatedOrder!.Status.Should().BeOneOf(OrderStatus.Processing, OrderStatus.Completed);

        // If completed, verify event was published
        if (updatedOrder.Status == OrderStatus.Completed)
        {
            await publishEndpoint.Received(1).Publish(
                Arg.Is<OrderCompletedEvent>(e => e.OrderId == order.Id),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Consume_HandlesNonExistentOrder_Gracefully()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        var nonExistentOrderId = Guid.NewGuid();
        var orderCreatedEvent = new OrderCreatedEvent(
            nonExistentOrderId,
            Guid.NewGuid(),
            100m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        var act = async () => await consumer.Consume(consumeContext);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
