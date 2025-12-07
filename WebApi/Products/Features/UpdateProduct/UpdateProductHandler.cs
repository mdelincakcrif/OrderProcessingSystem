using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Products.DTOs;

namespace WebApi.Products.Features.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public UpdateProductHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Results.NotFound(new { error = "Product not found" });
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await _context.SaveChangesAsync(cancellationToken);

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
