using MediatR;

namespace WebApi.Orders.Features.DeleteOrder;

public record DeleteOrderCommand(Guid Id) : IRequest<IResult>;
