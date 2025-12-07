using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.TestHelpers;
using WebApi.Users.DTOs;
using WebApi.Users.Features.CreateUser;
using Xunit;

namespace WebApi.Tests;

public class CreateUserHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_CreatesUserAndReturnsCreated()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateUserHandler(context);
        var command = new CreateUserCommand("Test User", "test@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify user was created in database
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        user.Should().NotBeNull();
        user!.Name.Should().Be("Test User");
        user.Email.Should().Be("test@example.com");

        // Verify password is hashed
        user.Password.Should().NotBe("password123");
        BCrypt.Net.BCrypt.Verify("password123", user.Password).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateUserHandler(context);

        // Create first user
        var command1 = new CreateUserCommand("User One", "duplicate@example.com", "password123");
        await handler.Handle(command1, CancellationToken.None);

        // Try to create second user with same email
        var command2 = new CreateUserCommand("User Two", "duplicate@example.com", "password456");

        // Act
        var result = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify only one user exists
        var userCount = await context.Users.CountAsync(u => u.Email == "duplicate@example.com");
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_PasswordIsHashed_NotStoredInPlainText()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new CreateUserHandler(context);
        var plainPassword = "mySecretPassword123";
        var command = new CreateUserCommand("Secure User", "secure@example.com", plainPassword);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "secure@example.com");
        user.Should().NotBeNull();
        user!.Password.Should().NotBe(plainPassword);
        user.Password.Should().StartWith("$2a$"); // BCrypt hash prefix
    }
}
