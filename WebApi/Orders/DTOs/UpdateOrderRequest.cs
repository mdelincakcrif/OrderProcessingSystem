using WebApi.Orders.Domain;

namespace WebApi.Orders.DTOs;

public record UpdateOrderRequest(
    OrderStatus Status,
    List<CreateOrderItemRequest> Items
);
