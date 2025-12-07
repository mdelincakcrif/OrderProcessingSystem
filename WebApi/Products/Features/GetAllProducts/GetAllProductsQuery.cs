using MediatR;

namespace WebApi.Products.Features.GetAllProducts;

public record GetAllProductsQuery() : IRequest<IResult>;
