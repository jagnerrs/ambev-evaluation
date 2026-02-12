using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for AuthenticateUserHandler.
/// </summary>
public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact(DisplayName = "Authenticate with valid credentials returns token")]
    public async Task Handle_ValidCredentials_ReturnsAuthenticateUserResult()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "Test@1234" };
        const string token = "jwt-token";

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Username);
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact(DisplayName = "Authenticate with invalid password throws UnauthorizedAccessException")]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var user = UserTestData.GenerateValidUser();
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "WrongPassword" };

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact(DisplayName = "Authenticate with non-existing user throws UnauthorizedAccessException")]
    public async Task Handle_NonExistingUser_ThrowsUnauthorizedAccessException()
    {
        var command = new AuthenticateUserCommand { Email = "nonexistent@example.com", Password = "Test@1234" };

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact(DisplayName = "Authenticate with inactive user throws UnauthorizedAccessException")]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Inactive;
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "Test@1234" };

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not active");
    }
}
