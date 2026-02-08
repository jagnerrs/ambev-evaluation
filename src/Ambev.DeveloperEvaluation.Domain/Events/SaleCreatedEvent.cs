using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is created.
/// </summary>
public class SaleCreatedEvent
{
    public Sale Sale { get; }

    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
    }
}
