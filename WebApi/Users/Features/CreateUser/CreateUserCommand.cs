using MediatR;

namespace WebApi.Users.Features.CreateUser;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password
) : IRequest<IResult>;
