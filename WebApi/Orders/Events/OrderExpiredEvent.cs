namespace WebApi.Orders.Events;

public record OrderExpiredEvent(
    Guid OrderId,
    Guid UserId,
    DateTime ExpiredAt
);
