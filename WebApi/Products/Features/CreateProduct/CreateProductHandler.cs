using MediatR;
using WebApi.Dal;
using WebApi.Products.Domain;
using WebApi.Products.DTOs;

namespace WebApi.Products.Features.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public CreateProductHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt
        );

        return Results.Created($"/api/products/{product.Id}", response);
    }
}
