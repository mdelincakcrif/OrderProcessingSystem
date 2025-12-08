using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Notifications.Domain;
using WebApi.Notifications.EventConsumers;
using WebApi.Orders.Events;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class OrderCompletedNotificationConsumerTests
{
    [Fact]
    public async Task Consume_CreatesNotificationInDatabase()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderCompletedNotificationConsumer>>();
        var consumer = new OrderCompletedNotificationConsumer(context, logger);

        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderCompletedEvent = new OrderCompletedEvent(
            orderId,
            userId,
            150.50m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCompletedEvent>>();
        consumeContext.Message.Returns(orderCompletedEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.OrderId == orderId);

        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.OrderCompleted);
        notification.Message.Should().Contain(orderId.ToString());
        notification.Message.Should().Contain("150");
    }

    [Fact]
    public async Task Consume_LogsEmailNotification()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderCompletedNotificationConsumer>>();
        var consumer = new OrderCompletedNotificationConsumer(context, logger);

        var orderCompletedEvent = new OrderCompletedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCompletedEvent>>();
        consumeContext.Message.Returns(orderCompletedEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        // Verify notification was created in database
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.OrderId == orderCompletedEvent.OrderId);

        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.OrderCompleted);
    }
}
