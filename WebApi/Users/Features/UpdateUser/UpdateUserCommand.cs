using MediatR;

namespace WebApi.Users.Features.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email
) : IRequest<IResult>;
