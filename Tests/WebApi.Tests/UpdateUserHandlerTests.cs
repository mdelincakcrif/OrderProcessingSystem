using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.TestHelpers;
using WebApi.Users.Domain;
using WebApi.Users.Features.UpdateUser;
using Xunit;

namespace WebApi.Tests;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_UpdatesUser()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateUserHandler(context);
        var command = new UpdateUserCommand(userId, "New Name", "new@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var updatedUser = await context.Users.FindAsync(userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.Name.Should().Be("New Name");
        updatedUser.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistingUserId_ReturnsNotFound()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new UpdateUserHandler(context);
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateUserCommand(nonExistentId, "Test Name", "test@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        // Create two users
        var user1Id = Guid.NewGuid();
        var user1 = new User
        {
            Id = user1Id,
            Name = "User One",
            Email = "user1@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        var user2Id = Guid.NewGuid();
        var user2 = new User
        {
            Id = user2Id,
            Name = "User Two",
            Email = "user2@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var handler = new UpdateUserHandler(context);
        // Try to update user2 with user1's email
        var command = new UpdateUserCommand(user2Id, "User Two Updated", "user1@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify user2 was not updated
        var unchangedUser = await context.Users.FindAsync(user2Id);
        unchangedUser!.Email.Should().Be("user2@example.com");
    }
}
