namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Request to create a new sale.
/// </summary>
public class CreateSaleRequest
{
    /// <summary>
    /// Date when the sale was made.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Customer external identity.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Customer name (denormalized).
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Branch external identity.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Branch name (denormalized).
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Sale line items.
    /// </summary>
    public List<CreateSaleItemRequest> Items { get; set; } = new();
}
