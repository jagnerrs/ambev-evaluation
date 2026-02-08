namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity reference for Customer domain entity with denormalized description.
/// </summary>
public record CustomerReference
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
}
