namespace WebApi.Orders.DTOs;

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    decimal Price
);
