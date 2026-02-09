using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

/// <summary>
/// Unit tests for SaleDiscountService.
/// </summary>
public class SaleDiscountServiceTests
{
    private readonly SaleDiscountService _sut = new();

    [Theory(DisplayName = "Quantity below 4 should have 0% discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateDiscountPercent_QuantityBelow4_ReturnsZero(int quantity)
    {
        var result = _sut.CalculateDiscountPercent(quantity);
        result.Should().Be(0);
    }

    [Theory(DisplayName = "Quantity 4-9 should have 10% discount")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void CalculateDiscountPercent_Quantity4To9_Returns10(int quantity)
    {
        var result = _sut.CalculateDiscountPercent(quantity);
        result.Should().Be(10);
    }

    [Theory(DisplayName = "Quantity 10-20 should have 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void CalculateDiscountPercent_Quantity10To20_Returns20(int quantity)
    {
        var result = _sut.CalculateDiscountPercent(quantity);
        result.Should().Be(20);
    }

    [Fact(DisplayName = "Quantity above 20 should throw DomainException")]
    public void CalculateDiscountPercent_QuantityAbove20_ThrowsDomainException()
    {
        var act = () => _sut.CalculateDiscountPercent(21);
        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }
}
