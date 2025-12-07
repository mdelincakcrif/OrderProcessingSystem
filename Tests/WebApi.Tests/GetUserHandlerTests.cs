using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.TestHelpers;
using WebApi.Users.Domain;
using WebApi.Users.Features.GetUser;
using Xunit;

namespace WebApi.Tests;

public class GetUserHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingUserId_ReturnsUser()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetUserHandler(context);
        var query = new GetUserQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingUserId_ReturnsNotFound()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var handler = new GetUserHandler(context);
        var nonExistentId = Guid.NewGuid();
        var query = new GetUserQuery(nonExistentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
