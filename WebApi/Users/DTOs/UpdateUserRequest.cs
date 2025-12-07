namespace WebApi.Users.DTOs;

public record UpdateUserRequest(
    string Name,
    string Email
);
