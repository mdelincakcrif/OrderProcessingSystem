using MediatR;
using WebApi.Users.Features.CreateUser;
using WebApi.Users.Features.GetUser;
using WebApi.Users.Features.GetAllUsers;
using WebApi.Users.Features.UpdateUser;
using WebApi.Users.Features.DeleteUser;
using WebApi.Users.DTOs;

namespace WebApi.Users.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapPost("/", async (CreateUserRequest request, IMediator mediator) =>
        {
            var command = new CreateUserCommand(request.Name, request.Email, request.Password);
            return await mediator.Send(command);
        })
        .WithName("CreateUser");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetUserQuery(id);
            return await mediator.Send(query);
        })
        .WithName("GetUser");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetAllUsersQuery();
            return await mediator.Send(query);
        })
        .WithName("GetAllUsers");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IMediator mediator) =>
        {
            var command = new UpdateUserCommand(id, request.Name, request.Email);
            return await mediator.Send(command);
        })
        .WithName("UpdateUser");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteUserCommand(id);
            return await mediator.Send(command);
        })
        .WithName("DeleteUser");

        return app;
    }
}
