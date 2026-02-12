using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for DeleteUserHandler.
/// </summary>
public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new DeleteUserHandler(_userRepository);
    }

    [Fact(DisplayName = "DeleteUser with existing user returns success")]
    public async Task Handle_ExistingUser_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>()).Returns(true);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        await _userRepository.Received(1).DeleteAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "DeleteUser with non-existing user throws KeyNotFoundException")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found");
    }

    [Fact(DisplayName = "DeleteUser with invalid Id throws ValidationException")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var command = new DeleteUserCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
