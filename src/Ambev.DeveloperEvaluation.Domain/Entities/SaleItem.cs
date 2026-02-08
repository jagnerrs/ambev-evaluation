namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a line item in a sale with product reference and quantity-based pricing.
/// </summary>
public class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsCancelled { get; set; }

    public Sale Sale { get; set; } = null!;
}
