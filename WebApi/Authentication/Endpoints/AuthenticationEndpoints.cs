using MediatR;
using WebApi.Authentication.Features.Login;
using WebApi.Authentication.DTOs;

namespace WebApi.Authentication.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IMediator mediator) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            return await mediator.Send(command);
        })
        .WithName("Login");

        return app;
    }
}
