namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

/// <summary>
/// Request for cancelling a sale item.
/// When Quantity is not provided or equals the item quantity, performs full cancellation.
/// When Quantity is provided and less than the item quantity, performs partial cancellation
/// and recalculates the discount based on the remaining quantity.
/// </summary>
public class CancelSaleItemRequest
{
    /// <summary>
    /// Quantity to cancel. If not provided, cancels the entire item.
    /// </summary>
    public int? Quantity { get; set; }
}
