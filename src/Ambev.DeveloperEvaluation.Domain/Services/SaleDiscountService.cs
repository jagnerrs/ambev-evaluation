using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Implementation of quantity-based discount rules:
/// - Below 4 items: no discount
/// - 4-9 items: 10% discount
/// - 10-20 items: 20% discount
/// - Above 20: not allowed
/// </summary>
public class SaleDiscountService : ISaleDiscountService
{
    private const int MinQuantityForDiscount = 4;
    private const int MinQuantityForHigherDiscount = 10;
    private const int MaxQuantity = 20;
    private const decimal Discount10Percent = 10m;
    private const decimal Discount20Percent = 20m;

    /// <inheritdoc />
    public decimal CalculateDiscountPercent(int quantity)
    {
        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items per product.");

        if (quantity < MinQuantityForDiscount)
            return 0;

        if (quantity >= MinQuantityForHigherDiscount)
            return Discount20Percent;

        return Discount10Percent;
    }
}
