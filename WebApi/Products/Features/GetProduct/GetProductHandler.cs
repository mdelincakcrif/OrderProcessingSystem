using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Products.DTOs;

namespace WebApi.Products.Features.GetProduct;

public class GetProductHandler : IRequestHandler<GetProductQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetProductHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Results.NotFound(new { error = "Product not found" });
        }

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt
        );

        return Results.Ok(response);
    }
}
