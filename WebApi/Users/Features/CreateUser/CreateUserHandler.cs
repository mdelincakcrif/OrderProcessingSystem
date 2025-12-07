using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Users.Domain;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public CreateUserHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Results.BadRequest(new { error = "Email already exists" });
        }

        // Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Password = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(user.Id, user.Name, user.Email);
        return Results.Created($"/api/users/{user.Id}", response);
    }
}
