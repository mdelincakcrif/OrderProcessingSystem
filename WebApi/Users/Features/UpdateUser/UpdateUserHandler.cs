using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public UpdateUserHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Check if email is already taken by another user
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != request.Id, cancellationToken);

        if (emailExists)
        {
            return Results.BadRequest(new { error = "Email already exists" });
        }

        user.Name = request.Name;
        user.Email = request.Email;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(user.Id, user.Name, user.Email);
        return Results.Ok(response);
    }
}
