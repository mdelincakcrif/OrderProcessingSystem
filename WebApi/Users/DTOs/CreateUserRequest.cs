namespace WebApi.Users.DTOs;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);
