using MediatR;

namespace WebApi.Products.Features.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock
) : IRequest<IResult>;
