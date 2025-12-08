namespace WebApi.Orders.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    DateTime CreatedAt
);
