using MassTransit;
using WebApi.Dal;
using WebApi.Notifications.Domain;
using WebApi.Orders.Events;

namespace WebApi.Notifications.EventConsumers;

public class OrderCompletedNotificationConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly ILogger<OrderCompletedNotificationConsumer> _logger;

    public OrderCompletedNotificationConsumer(
        OrderProcessingDbContext context,
        ILogger<OrderCompletedNotificationConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        // Mock email notification (just log to console)
        _logger.LogInformation(
            "📧 Sending email notification: Order {OrderId} for User {UserId} has been completed. Total: ${Total}",
            message.OrderId,
            message.UserId,
            message.Total);

        // Save notification to database (audit trail)
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            Type = NotificationType.OrderCompleted,
            Message = $"Order {message.OrderId} completed successfully. Total: ${message.Total}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification saved to database for Order {OrderId}", message.OrderId);
    }
}
