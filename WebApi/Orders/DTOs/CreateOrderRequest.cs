namespace WebApi.Orders.DTOs;

public record CreateOrderRequest(
    Guid UserId,
    List<CreateOrderItemRequest> Items
);
