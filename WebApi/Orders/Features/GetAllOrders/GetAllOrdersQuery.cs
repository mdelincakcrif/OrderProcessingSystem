using MediatR;

namespace WebApi.Orders.Features.GetAllOrders;

public record GetAllOrdersQuery() : IRequest<IResult>;
