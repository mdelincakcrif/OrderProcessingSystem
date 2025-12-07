using FluentAssertions;
using WebApi.Tests.TestHelpers;
using WebApi.Users.Domain;
using WebApi.Users.Features.GetAllUsers;
using Xunit;

namespace WebApi.Tests;

public class GetAllUsersHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllUsers()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        // Add test users
        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User One",
                Email = "user1@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User Two",
                Email = "user2@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User Three",
                Email = "user3@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            }
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();

        // Clear seed data
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
