using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is modified.
/// </summary>
public class SaleModifiedEvent
{
    public Sale Sale { get; }

    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale;
    }
}
