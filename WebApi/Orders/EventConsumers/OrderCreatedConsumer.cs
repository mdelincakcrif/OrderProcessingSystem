using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;

namespace WebApi.Orders.EventConsumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        OrderProcessingDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderCreatedConsumer> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing OrderCreatedEvent for Order {OrderId}", message.OrderId);

        // Find the order
        var order = await _context.Orders.FindAsync(message.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", message.OrderId);
            return;
        }

        // Update status to Processing
        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} status updated to Processing", message.OrderId);

        // Simulate payment processing (5 second delay)
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // 50% success rate
        var random = new Random();
        var isSuccessful = random.Next(0, 2) == 1;

        if (isSuccessful)
        {
            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} completed successfully", message.OrderId);

            // Publish OrderCompletedEvent
            await _publishEndpoint.Publish(new OrderCompletedEvent(
                order.Id,
                order.UserId,
                order.Total,
                order.UpdatedAt
            ), context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} remains in Processing state (payment pending)", message.OrderId);
        }
    }
}
