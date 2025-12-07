using MediatR;

namespace WebApi.Users.Features.GetAllUsers;

public record GetAllUsersQuery() : IRequest<IResult>;
