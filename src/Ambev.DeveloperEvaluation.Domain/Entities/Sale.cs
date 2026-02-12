namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale transaction with customer, branch, and line items.
/// Uses External Identities pattern for Customer and Branch references.
/// </summary>
public class Sale
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    /// <summary>
    /// Cancels the entire sale.
    /// </summary>
    public void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;

        foreach (var item in SaleItems)
        {
            if (!item.IsCancelled)
                item.IsCancelled = true;
        }
    }

    /// <summary>
    /// Cancels a specific item within the sale (full cancellation).
    /// </summary>
    /// <param name="itemId">The ID of the item to cancel.</param>
    public void CancelItem(Guid itemId)
    {
        var item = SaleItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        item.IsCancelled = true;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reduces the quantity of an item and recalculates its discount and line total.
    /// Caller must provide the new discount percent and line total based on the remaining quantity.
    /// If remaining quantity is 0, the item is marked as cancelled.
    /// </summary>
    public void ReduceItemQuantity(Guid itemId, int newQuantity, decimal newDiscountPercent, decimal newLineTotal)
    {
        var item = SaleItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        item.Quantity = newQuantity;
        item.DiscountPercent = newDiscountPercent;
        item.LineTotal = newLineTotal;
        if (newQuantity <= 0)
            item.IsCancelled = true;

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates the total amount based on non-cancelled items.
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = SaleItems
            .Where(i => !i.IsCancelled)
            .Sum(i => i.LineTotal);
    }
}
