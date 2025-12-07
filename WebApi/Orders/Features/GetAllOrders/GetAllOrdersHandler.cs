using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Features.GetAllOrders;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetAllOrdersHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        var response = orders.Select(order => new OrderResponse(
            order.Id,
            order.UserId,
            order.Total,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.Quantity, i.Price)).ToList()
        )).ToList();

        return Results.Ok(response);
    }
}
