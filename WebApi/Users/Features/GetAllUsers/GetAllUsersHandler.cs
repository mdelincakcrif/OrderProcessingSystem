using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetAllUsersHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToListAsync(cancellationToken);

        return Results.Ok(users);
    }
}
