using MediatR;

namespace WebApi.Products.Features.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<IResult>;
