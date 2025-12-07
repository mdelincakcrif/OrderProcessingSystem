using MediatR;

namespace WebApi.Authentication.Features.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<IResult>;
