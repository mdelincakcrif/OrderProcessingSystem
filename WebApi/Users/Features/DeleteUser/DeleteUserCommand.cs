using MediatR;

namespace WebApi.Users.Features.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<IResult>;
