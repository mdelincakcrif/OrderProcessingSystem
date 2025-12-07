using MediatR;
using WebApi.Orders.Domain;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.UpdateOrder;

public record UpdateOrderCommand(
    Guid Id,
    OrderStatus Status,
    List<CreateOrderItemRequest> Items
) : IRequest<IResult>;
