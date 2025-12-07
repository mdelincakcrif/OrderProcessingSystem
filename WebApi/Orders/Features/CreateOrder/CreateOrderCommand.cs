using MediatR;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    List<CreateOrderItemRequest> Items
) : IRequest<IResult>;
