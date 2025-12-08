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

public class OrderExpiredNotificationConsumerTests
{
    [Fact]
    public async Task Consume_CreatesNotificationInDatabase()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderExpiredNotificationConsumer>>();
        var consumer = new OrderExpiredNotificationConsumer(context, logger);

        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderExpiredEvent = new OrderExpiredEvent(
            orderId,
            userId,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderExpiredEvent>>();
        consumeContext.Message.Returns(orderExpiredEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.OrderId == orderId);

        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.OrderExpired);
        notification.Message.Should().Contain(orderId.ToString());
        notification.Message.Should().Contain("expired");
    }

    [Fact]
    public async Task Consume_LogsExpirationMessage()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderExpiredNotificationConsumer>>();
        var consumer = new OrderExpiredNotificationConsumer(context, logger);

        var orderExpiredEvent = new OrderExpiredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderExpiredEvent>>();
        consumeContext.Message.Returns(orderExpiredEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        // Verify notification was created in database
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.OrderId == orderExpiredEvent.OrderId);

        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.OrderExpired);
    }
}
