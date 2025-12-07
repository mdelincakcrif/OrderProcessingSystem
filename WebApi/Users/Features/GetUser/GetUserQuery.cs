using MediatR;

namespace WebApi.Users.Features.GetUser;

public record GetUserQuery(Guid Id) : IRequest<IResult>;
