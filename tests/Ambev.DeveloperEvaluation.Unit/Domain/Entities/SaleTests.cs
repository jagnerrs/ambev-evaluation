using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Sale entity.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Cancel should set IsCancelled to true")]
    public void Cancel_WhenCalled_SetsIsCancelledTrue()
    {
        var sale = CreateSaleWithItems();
        sale.Cancel();
        sale.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "CancelItem should cancel specific item and recalculate total")]
    public void CancelItem_WhenCalled_CancelsItemAndRecalculatesTotal()
    {
        var sale = CreateSaleWithItems();
        var itemToCancel = sale.SaleItems.First();
        var initialTotal = sale.TotalAmount;

        sale.CancelItem(itemToCancel.Id);

        itemToCancel.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(initialTotal - itemToCancel.LineTotal);
    }

    [Fact(DisplayName = "RecalculateTotal should sum non-cancelled items")]
    public void RecalculateTotal_WhenCalled_SumsNonCancelledItems()
    {
        var sale = CreateSaleWithItems();
        sale.SaleItems.First().IsCancelled = true;
        sale.RecalculateTotal();

        var expectedTotal = sale.SaleItems.Where(i => !i.IsCancelled).Sum(i => i.LineTotal);
        sale.TotalAmount.Should().Be(expectedTotal);
    }

    private static Sale CreateSaleWithItems()
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-20250001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch",
            IsCancelled = false
        };

        var item1 = new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Product 1",
            Quantity = 2,
            UnitPrice = 10,
            DiscountPercent = 0,
            LineTotal = 20,
            IsCancelled = false
        };
        var item2 = new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Product 2",
            Quantity = 5,
            UnitPrice = 10,
            DiscountPercent = 10,
            LineTotal = 45,
            IsCancelled = false
        };

        sale.SaleItems.Add(item1);
        sale.SaleItems.Add(item2);
        sale.TotalAmount = 65;
        return sale;
    }
}
