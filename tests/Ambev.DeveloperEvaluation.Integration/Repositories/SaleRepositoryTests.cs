using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Integration tests for SaleRepository.
/// </summary>
public class SaleRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly SaleRepository _sut;

    public SaleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _context.Database.EnsureCreated();
        _sut = new SaleRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact(DisplayName = "CreateAsync should persist sale with items")]
    public async Task CreateAsync_ValidSale_PersistsSale()
    {
        var sale = CreateSale();
        var created = await _sut.CreateAsync(sale);

        created.Id.Should().NotBeEmpty();
        created.SaleNumber.Should().Be(sale.SaleNumber);

        var retrieved = await _sut.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.SaleItems.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GetNextSaleNumberAsync should return sequential numbers")]
    public async Task GetNextSaleNumberAsync_ReturnsSequentialNumbers()
    {
        var number1 = await _sut.GetNextSaleNumberAsync();
        number1.Should().StartWith("SALE-");

        await _sut.CreateAsync(CreateSaleWithNumber(number1));

        var number2 = await _sut.GetNextSaleNumberAsync();
        number2.Should().StartWith("SALE-");
        number2.Should().NotBe(number1);
    }

    private static Sale CreateSaleWithNumber(string saleNumber)
    {
        var sale = CreateSale();
        sale.SaleNumber = saleNumber;
        return sale;
    }

    [Fact(DisplayName = "GetByIdAsync should return sale with items when exists")]
    public async Task GetByIdAsync_ExistingSale_ReturnsSaleWithItems()
    {
        var sale = CreateSale();
        await _sut.CreateAsync(sale);

        var result = await _sut.GetByIdAsync(sale.Id);

        result.Should().NotBeNull();
        result!.SaleNumber.Should().Be(sale.SaleNumber);
        result.SaleItems.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GetByIdAsync should return null when sale does not exist")]
    public async Task GetByIdAsync_NonExistingSale_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetBySaleNumberAsync should return sale when exists")]
    public async Task GetBySaleNumberAsync_ExistingSale_ReturnsSale()
    {
        var sale = CreateSale();
        sale.SaleNumber = "SALE-20250099";
        await _sut.CreateAsync(sale);

        var result = await _sut.GetBySaleNumberAsync("SALE-20250099");

        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.SaleItems.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GetBySaleNumberAsync should return null when sale number does not exist")]
    public async Task GetBySaleNumberAsync_NonExistingSaleNumber_ReturnsNull()
    {
        var result = await _sut.GetBySaleNumberAsync("SALE-20999999");

        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetAllAsync should return sales ordered by SaleDate descending")]
    public async Task GetAllAsync_ReturnsSalesOrderedBySaleDateDescending()
    {
        var sale1 = CreateSale();
        sale1.SaleDate = DateTime.UtcNow.AddDays(-2);
        var sale2 = CreateSale();
        sale2.SaleDate = DateTime.UtcNow.AddDays(-1);
        await _sut.CreateAsync(sale1);
        await _sut.CreateAsync(sale2);

        var (items, totalCount) = await _sut.GetAllAsync(1, 10);

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
        items.First().Id.Should().Be(sale2.Id);
    }

    [Fact(DisplayName = "UpdateAsync should persist changes")]
    public async Task UpdateAsync_ExistingSale_UpdatesSuccessfully()
    {
        var sale = CreateSale();
        await _sut.CreateAsync(sale);

        sale.CustomerName = "Updated Customer";
        sale.UpdatedAt = DateTime.UtcNow;

        var updated = await _sut.UpdateAsync(sale);

        updated.CustomerName.Should().Be("Updated Customer");
        var retrieved = await _sut.GetByIdAsync(sale.Id);
        retrieved!.CustomerName.Should().Be("Updated Customer");
    }

    [Fact(DisplayName = "DeleteAsync should remove sale")]
    public async Task DeleteAsync_ExistingSale_RemovesSale()
    {
        var sale = CreateSale();
        await _sut.CreateAsync(sale);
        var result = await _sut.DeleteAsync(sale.Id);
        result.Should().BeTrue();

        var retrieved = await _sut.GetByIdAsync(sale.Id);
        retrieved.Should().BeNull();
    }

    private static Sale CreateSale()
    {
        var saleId = Guid.NewGuid();
        return new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-20250001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            TotalAmount = 90,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow,
            SaleItems =
            [
                new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 10,
                    UnitPrice = 10,
                    DiscountPercent = 20,
                    LineTotal = 80,
                    IsCancelled = false
                }
            ]
        };
    }
}
