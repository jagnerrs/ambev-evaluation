namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Domain service for applying quantity-based discount rules.
/// </summary>
public interface ISaleDiscountService
{
    /// <summary>
    /// Calculates the discount percentage for a given quantity.
    /// </summary>
    /// <param name="quantity">The quantity of identical items.</param>
    /// <returns>0 for &lt;4 items, 10 for 4-9, 20 for 10-20.</returns>
    /// <exception cref="DomainException">Thrown when quantity exceeds 20.</exception>
    decimal CalculateDiscountPercent(int quantity);
}
