using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Security;

/// <summary>
/// Unit tests for BCryptPasswordHasher.
/// </summary>
public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact(DisplayName = "HashPassword returns different hash for same password")]
    public void HashPassword_SamePassword_ReturnsDifferentHashesEachTime()
    {
        var password = "Test@1234";

        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "HashPassword returns valid BCrypt hash format")]
    public void HashPassword_ValidPassword_ReturnsBCryptFormat()
    {
        var password = "Test@1234";

        var hash = _sut.HashPassword(password);

        hash.Should().StartWith("$2");
    }

    [Fact(DisplayName = "VerifyPassword returns true when password matches hash")]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var password = "Test@1234";
        var hash = _sut.HashPassword(password);

        var result = _sut.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "VerifyPassword returns false when password does not match")]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("CorrectPassword");

        var result = _sut.VerifyPassword("WrongPassword", hash);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "VerifyPassword returns false when hash does not match password")]
    public void VerifyPassword_HashFromDifferentPassword_ReturnsFalse()
    {
        var hashOfOtherPassword = _sut.HashPassword("OtherPassword@123");

        var result = _sut.VerifyPassword("Test@1234", hashOfOtherPassword);

        result.Should().BeFalse();
    }
}
