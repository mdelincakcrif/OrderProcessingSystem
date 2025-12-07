namespace WebApi.Authentication.DTOs;

public record LoginRequest(
    string Email,
    string Password
);
