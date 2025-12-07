using MediatR;
using WebApi.Orders.Features.CreateOrder;
using WebApi.Orders.Features.GetOrder;
using WebApi.Orders.Features.GetAllOrders;
using WebApi.Orders.Features.UpdateOrder;
using WebApi.Orders.Features.DeleteOrder;
using WebApi.Orders.DTOs;

namespace WebApi.Orders.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", async (CreateOrderRequest request, IMediator mediator) =>
        {
            var command = new CreateOrderCommand(request.UserId, request.Items);
            return await mediator.Send(command);
        })
        .WithName("CreateOrder");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetOrderQuery(id);
            return await mediator.Send(query);
        })
        .WithName("GetOrder");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetAllOrdersQuery();
            return await mediator.Send(query);
        })
        .WithName("GetAllOrders");

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, IMediator mediator) =>
        {
            var command = new UpdateOrderCommand(id, request.Status, request.Items);
            return await mediator.Send(command);
        })
        .WithName("UpdateOrder");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteOrderCommand(id);
            return await mediator.Send(command);
        })
        .WithName("DeleteOrder");

        return app;
    }
}
