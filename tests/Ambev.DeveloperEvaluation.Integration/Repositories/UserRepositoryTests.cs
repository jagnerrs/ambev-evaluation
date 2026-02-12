using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Integration tests for UserRepository.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _context.Database.EnsureCreated();
        _sut = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact(DisplayName = "CreateAsync should persist user")]
    public async Task CreateAsync_ValidUser_PersistsUser()
    {
        var user = CreateUser();

        var created = await _sut.CreateAsync(user);

        created.Id.Should().NotBeEmpty();
        var retrieved = await _sut.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "GetByIdAsync should return user when exists")]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = CreateUser();
        await _sut.CreateAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "GetByIdAsync should return null when user does not exist")]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetByEmailAsync should return user when exists")]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        var user = CreateUser();
        await _sut.CreateAsync(user);

        var result = await _sut.GetByEmailAsync(user.Email);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact(DisplayName = "GetByEmailAsync should return null when email does not exist")]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        var result = await _sut.GetByEmailAsync("nonexistent@example.com");

        result.Should().BeNull();
    }

    [Fact(DisplayName = "DeleteAsync should remove user")]
    public async Task DeleteAsync_ExistingUser_RemovesUser()
    {
        var user = CreateUser();
        await _sut.CreateAsync(user);

        var result = await _sut.DeleteAsync(user.Id);

        result.Should().BeTrue();
        var retrieved = await _sut.GetByIdAsync(user.Id);
        retrieved.Should().BeNull();
    }

    [Fact(DisplayName = "DeleteAsync should return false when user does not exist")]
    public async Task DeleteAsync_NonExistingUser_ReturnsFalse()
    {
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "GetAllAsync should return paginated users")]
    public async Task GetAllAsync_WithUsers_ReturnsPaginated()
    {
        await _sut.CreateAsync(CreateUser());
        await _sut.CreateAsync(CreateUser());

        var (items, totalCount) = await _sut.GetAllAsync(1, 10);

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetAllAsync should respect pagination")]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        await _sut.CreateAsync(CreateUser());
        await _sut.CreateAsync(CreateUser());
        await _sut.CreateAsync(CreateUser());

        var (items, totalCount) = await _sut.GetAllAsync(2, 2);

        totalCount.Should().Be(3);
        items.Should().HaveCount(1);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Phone = "+5511999999999",
            Password = "hashedPassword",
            Status = UserStatus.Active,
            Role = UserRole.Manager
        };
    }
}
