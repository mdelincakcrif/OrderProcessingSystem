using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.GetOrder;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetOrderHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Results.NotFound(new { error = "Order not found" });
        }

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
