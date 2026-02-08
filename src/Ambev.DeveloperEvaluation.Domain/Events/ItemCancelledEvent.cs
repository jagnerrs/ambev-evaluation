using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale item is cancelled.
/// </summary>
public class ItemCancelledEvent
{
    public Sale Sale { get; }
    public SaleItem Item { get; }

    public ItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
    }
}
