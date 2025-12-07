using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Security;
using WebApi.Dal;
using WebApi.Authentication.DTOs;

namespace WebApi.Authentication.Features.Login;

public class LoginHandler : IRequestHandler<LoginCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public LoginHandler(OrderProcessingDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Results.Unauthorized();
        }

        var token = _jwtTokenService.GenerateToken(user);
        var response = new LoginResponse(token);

        return Results.Ok(response);
    }
}
