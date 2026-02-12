using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for ListUsersHandler.
/// </summary>
public class ListUsersHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly ListUsersHandler _handler;

    public ListUsersHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new ListUsersHandler(_userRepository);
    }

    [Fact(DisplayName = "ListUsers returns paginated result")]
    public async Task Handle_ValidRequest_ReturnsListUsersResult()
    {
        var command = new ListUsersCommand(1, 10);
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "User1",
                Email = "user1@example.com",
                Phone = "+5511999999999",
                Role = UserRole.Manager,
                Status = UserStatus.Active
            }
        };

        _userRepository.GetAllAsync(1, 10, Arg.Any<CancellationToken>()).Returns((users, 1));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Username.Should().Be("User1");
        result.Items[0].Email.Should().Be("user1@example.com");
        result.TotalCount.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "ListUsers with invalid page throws ValidationException")]
    public async Task Handle_InvalidPage_ThrowsValidationException()
    {
        var command = new ListUsersCommand(0, 10);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "ListUsers with invalid pageSize throws ValidationException")]
    public async Task Handle_InvalidPageSize_ThrowsValidationException()
    {
        var command = new ListUsersCommand(1, 0);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "ListUsers with pageSize over 100 throws ValidationException")]
    public async Task Handle_PageSizeOver100_ThrowsValidationException()
    {
        var command = new ListUsersCommand(1, 101);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "ListUsers returns empty when no users")]
    public async Task Handle_NoUsers_ReturnsEmpty()
    {
        var command = new ListUsersCommand(1, 10);

        _userRepository.GetAllAsync(1, 10, Arg.Any<CancellationToken>()).Returns((new List<User>(), 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
