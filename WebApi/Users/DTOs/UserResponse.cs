namespace WebApi.Users.DTOs;

public record UserResponse(
    Guid Id,
    string Name,
    string Email
);
