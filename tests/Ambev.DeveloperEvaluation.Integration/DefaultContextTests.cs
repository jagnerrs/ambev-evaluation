using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

/// <summary>
/// Integration tests for DefaultContext and entity configurations.
/// </summary>
public class DefaultContextTests : IDisposable
{
    private readonly DefaultContext _context;

    public DefaultContextTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact(DisplayName = "EnsureCreated should build model successfully")]
    public void EnsureCreated_ValidConfiguration_Succeeds()
    {
        var act = () => _context.Database.EnsureCreated();

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "Should persist and retrieve User entity")]
    public async Task Context_CanPersistAndRetrieveUser()
    {
        _context.Database.EnsureCreated();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Email = "test@example.com",
            Phone = "+5511999999999",
            Password = "hashed",
            Status = UserStatus.Active,
            Role = UserRole.Manager
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Users.FindAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("test@example.com");
    }

    [Fact(DisplayName = "Should persist and retrieve Sale with SaleItems (cascade)")]
    public async Task Context_CanPersistAndRetrieveSaleWithItems()
    {
        _context.Database.EnsureCreated();

        var saleId = Guid.NewGuid();
        var sale = new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-20250001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            TotalAmount = 100,
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
                    Quantity = 2,
                    UnitPrice = 50,
                    DiscountPercent = 0,
                    LineTotal = 100,
                    IsCancelled = false
                }
            ]
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Sales
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        retrieved.Should().NotBeNull();
        retrieved!.SaleNumber.Should().Be("SALE-20250001");
        retrieved.SaleItems.Should().HaveCount(1);
        retrieved.SaleItems.First().ProductName.Should().Be("Product");
    }
}
