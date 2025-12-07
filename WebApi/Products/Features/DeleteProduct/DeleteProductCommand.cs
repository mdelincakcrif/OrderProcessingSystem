using MediatR;

namespace WebApi.Products.Features.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<IResult>;
