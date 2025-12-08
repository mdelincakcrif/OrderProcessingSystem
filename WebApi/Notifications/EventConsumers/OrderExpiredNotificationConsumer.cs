using MassTransit;
using WebApi.Dal;
using WebApi.Notifications.Domain;
using WebApi.Orders.Events;

namespace WebApi.Notifications.EventConsumers;

public class OrderExpiredNotificationConsumer : IConsumer<OrderExpiredEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly ILogger<OrderExpiredNotificationConsumer> _logger;

    public OrderExpiredNotificationConsumer(
        OrderProcessingDbContext context,
        ILogger<OrderExpiredNotificationConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderExpiredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Order {OrderId} expired and has been marked as expired",
            message.OrderId);

        // Save notification to database (audit trail only, no email)
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            Type = NotificationType.OrderExpired,
            Message = $"Order {message.OrderId} expired after 10 minutes in Processing status",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Expiration notification saved to database for Order {OrderId}", message.OrderId);
    }
}
