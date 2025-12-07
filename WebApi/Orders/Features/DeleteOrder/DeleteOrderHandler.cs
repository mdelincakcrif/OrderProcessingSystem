using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;

namespace WebApi.Orders.Features.DeleteOrder;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public DeleteOrderHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Results.NotFound(new { error = "Order not found" });
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
