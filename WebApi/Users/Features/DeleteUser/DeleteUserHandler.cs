using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;

namespace WebApi.Users.Features.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public DeleteUserHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
