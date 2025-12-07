using MediatR;
using WebApi.Products.Features.CreateProduct;
using WebApi.Products.Features.GetProduct;
using WebApi.Products.Features.GetAllProducts;
using WebApi.Products.Features.UpdateProduct;
using WebApi.Products.Features.DeleteProduct;
using WebApi.Products.DTOs;

namespace WebApi.Products.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapPost("/", async (CreateProductRequest request, IMediator mediator) =>
        {
            var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Stock);
            return await mediator.Send(command);
        })
        .WithName("CreateProduct");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetProductQuery(id);
            return await mediator.Send(query);
        })
        .WithName("GetProduct");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetAllProductsQuery();
            return await mediator.Send(query);
        })
        .WithName("GetAllProducts");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IMediator mediator) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Stock);
            return await mediator.Send(command);
        })
        .WithName("UpdateProduct");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteProductCommand(id);
            return await mediator.Send(command);
        })
        .WithName("DeleteProduct");

        return app;
    }
}
