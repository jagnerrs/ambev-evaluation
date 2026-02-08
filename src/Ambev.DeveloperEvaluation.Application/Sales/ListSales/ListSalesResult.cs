namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Result of ListSales operation.
/// </summary>
public class ListSalesResult
{
    public List<ListSaleItemResult> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

/// <summary>
/// Summary item in ListSales result.
/// </summary>
public class ListSaleItemResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
