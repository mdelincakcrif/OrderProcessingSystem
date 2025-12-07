using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Products.Features.CreateProduct;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_CreatesProduct()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            50
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var product = await context.Products.FirstOrDefaultAsync(p => p.Name == "Test Product");
        product.Should().NotBeNull();
        product!.Description.Should().Be("Test Description");
        product.Price.Should().Be(99.99m);
        product.Stock.Should().Be(50);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WithZeroPrice_CreatesProduct()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand(
            "Free Product",
            "Free item",
            0m,
            100
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var product = await context.Products.FirstOrDefaultAsync(p => p.Name == "Free Product");
        product.Should().NotBeNull();
        product!.Price.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WithZeroStock_CreatesProduct()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand(
            "Out of Stock Product",
            "Not available",
            49.99m,
            0
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var product = await context.Products.FirstOrDefaultAsync(p => p.Name == "Out of Stock Product");
        product.Should().NotBeNull();
        product!.Stock.Should().Be(0);
    }
}
