using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.GetUser;

public class GetUserHandler : IRequestHandler<GetUserQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetUserHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        var response = new UserResponse(user.Id, user.Name, user.Email);
        return Results.Ok(response);
    }
}
