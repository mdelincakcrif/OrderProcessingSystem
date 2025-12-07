using MediatR;

namespace WebApi.Orders.Features.GetOrder;

public record GetOrderQuery(Guid Id) : IRequest<IResult>;
