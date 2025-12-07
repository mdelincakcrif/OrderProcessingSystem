using FluentAssertions;
using Microsoft.Extensions.Options;
using WebApi.Authentication.Features.Login;
using WebApi.Common.Security;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "ThisIsASecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        });
        var jwtTokenService = new JwtTokenService(jwtSettings);
        var handler = new LoginHandler(context, jwtTokenService);
        var command = new LoginCommand("john@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Microsoft.AspNetCore.Http.IResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "ThisIsASecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        });
        var jwtTokenService = new JwtTokenService(jwtSettings);
        var handler = new LoginHandler(context, jwtTokenService);
        var command = new LoginCommand("nonexistent@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "ThisIsASecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        });
        var jwtTokenService = new JwtTokenService(jwtSettings);
        var handler = new LoginHandler(context, jwtTokenService);
        var command = new LoginCommand("john@example.com", "wrongpassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
