namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity reference for Branch domain entity with denormalized description.
/// </summary>
public record BranchReference
{
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
}
