using WebApi.Orders.Domain;

namespace WebApi.Orders.DTOs;

public record OrderResponse(
    Guid Id,
    Guid UserId,
    decimal Total,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemResponse> Items
);
