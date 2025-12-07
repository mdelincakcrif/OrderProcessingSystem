using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public CreateOrderHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Results.BadRequest(new { error = "User not found" });
        }

        // Verify all products exist
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (existingProducts.Count != productIds.Count)
        {
            return Results.BadRequest(new { error = "One or more products not found" });
        }

        // Calculate total
        var total = request.Items.Sum(i => i.Price * i.Quantity);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Total = total,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.Total,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.Quantity, i.Price)).ToList()
        );

        return Results.Created($"/api/orders/{order.Id}", response);
    }
}
