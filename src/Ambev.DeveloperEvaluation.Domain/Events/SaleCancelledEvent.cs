using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is cancelled.
/// </summary>
public class SaleCancelledEvent
{
    public Sale Sale { get; }

    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale;
    }
}
