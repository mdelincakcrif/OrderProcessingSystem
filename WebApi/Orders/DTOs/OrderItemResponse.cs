namespace WebApi.Orders.DTOs;

public record OrderItemResponse(
    int Id,
    Guid ProductId,
    int Quantity,
    decimal Price
);
