using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.TestHelpers;
using WebApi.Users.Domain;
using WebApi.Users.Features.DeleteUser;
using Xunit;

namespace WebApi.Tests;

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingUserId_DeletesUser()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "To Delete",
            Email = "delete@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new DeleteUserHandler(context);
        var command = new DeleteUserCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var deletedUser = await context.Users.FindAsync(userId);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingUserId_ReturnsNotFound()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new DeleteUserHandler(context);
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteUserCommand(nonExistentId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
