using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;

namespace Worker.Jobs;

public class OrderExpirationJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderExpirationJob> _logger;

    public OrderExpirationJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderExpirationJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderExpirationJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredOrders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired orders");
            }

            // Wait 60 seconds before next run
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

        _logger.LogInformation("OrderExpirationJob stopped");
    }

    private async Task ProcessExpiredOrders(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Find orders in Processing status older than 10 minutes
        var expirationCutoff = DateTime.UtcNow.AddMinutes(-10);
        var expiredOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Processing && o.UpdatedAt < expirationCutoff)
            .ToListAsync(cancellationToken);

        if (expiredOrders.Count == 0)
        {
            _logger.LogDebug("No expired orders found");
            return;
        }

        _logger.LogInformation("Found {Count} expired orders", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.Expired;
            order.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Expiring Order {OrderId}", order.Id);

            // Publish OrderExpiredEvent
            await publishEndpoint.Publish(new OrderExpiredEvent(
                order.Id,
                order.UserId,
                order.UpdatedAt
            ), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully expired {Count} orders", expiredOrders.Count);
    }
}
