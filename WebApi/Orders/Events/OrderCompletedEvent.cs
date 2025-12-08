namespace WebApi.Orders.Events;

public record OrderCompletedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    DateTime CompletedAt
);
