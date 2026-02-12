using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for GetUserHandler.
/// </summary>
public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "GetUser with existing user returns GetUserResult")]
    public async Task Handle_ExistingUser_ReturnsGetUserResult()
    {
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);
        var user = UserTestData.GenerateValidUser();
        user.Id = userId;
        var result = new GetUserResult
        {
            Id = user.Id,
            Name = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(userId);
        response.Email.Should().Be(user.Email);
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "GetUser with non-existing user throws KeyNotFoundException")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found");
    }

    [Fact(DisplayName = "GetUser with invalid Id throws ValidationException")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var command = new GetUserCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
