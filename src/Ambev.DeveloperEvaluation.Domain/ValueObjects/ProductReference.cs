namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity reference for Product domain entity with denormalized description.
/// </summary>
public record ProductReference
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
}
