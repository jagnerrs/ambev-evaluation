using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Security;

/// <summary>
/// Unit tests for JwtTokenGenerator.
/// </summary>
public class JwtTokenGeneratorTests
{
    private const string SecretKey = "ThisIsAVeryLongSecretKeyForJwtTokenGenerationTestsMinimum32Chars";

    [Fact(DisplayName = "GenerateToken returns valid JWT string")]
    public void GenerateToken_ValidUser_ReturnsJwtString()
    {
        var configuration = CreateConfiguration();
        var user = CreateUser("user-id", "john", "Admin");
        var sut = new JwtTokenGenerator(configuration);

        var token = sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact(DisplayName = "GenerateToken includes user claims")]
    public void GenerateToken_ValidUser_IncludesUserClaims()
    {
        var configuration = CreateConfiguration();
        var userId = Guid.NewGuid().ToString();
        var username = "testuser";
        var role = "Manager";
        var user = CreateUser(userId, username, role);
        var sut = new JwtTokenGenerator(configuration);

        var token = sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Value == userId);
        jwt.Claims.Should().Contain(c => c.Value == username);
        jwt.Claims.Should().Contain(c => c.Value == role);
    }

    [Fact(DisplayName = "GenerateToken throws when SecretKey is null")]
    public void GenerateToken_NullSecretKey_ThrowsArgumentException()
    {
        var configValues = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        var user = CreateUser("id", "user", "Admin");
        var sut = new JwtTokenGenerator(configuration);

        var act = () => sut.GenerateToken(user);

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "GenerateToken throws when SecretKey is empty")]
    public void GenerateToken_EmptySecretKey_ThrowsArgumentException()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = ""
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        var user = CreateUser("id", "user", "Admin");
        var sut = new JwtTokenGenerator(configuration);

        var act = () => sut.GenerateToken(user);

        act.Should().Throw<ArgumentException>();
    }

    private static IConfiguration CreateConfiguration()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = SecretKey
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    private static IUser CreateUser(string id, string username, string role)
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.Username.Returns(username);
        user.Role.Returns(role);
        return user;
    }
}
