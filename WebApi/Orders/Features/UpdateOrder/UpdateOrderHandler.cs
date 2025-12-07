using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.UpdateOrder;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public UpdateOrderHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Results.NotFound(new { error = "Order not found" });
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

        // Update order properties
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // Remove old items and add new ones
        _context.OrderItems.RemoveRange(order.Items);
        order.Items = request.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();

        // Recalculate total
        order.Total = order.Items.Sum(i => i.Price * i.Quantity);

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

        return Results.Ok(response);
    }
}
