using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Products.DTOs;

namespace WebApi.Products.Features.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetAllProductsHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(products);
    }
}
