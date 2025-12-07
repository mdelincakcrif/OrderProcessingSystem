using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;

namespace WebApi.Products.Features.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public DeleteProductHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Results.NotFound(new { error = "Product not found" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
