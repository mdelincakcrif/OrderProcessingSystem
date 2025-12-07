namespace WebApi.Products.DTOs;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);
