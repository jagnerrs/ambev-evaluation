using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for UpdateSaleHandler.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _discountService = Substitute.For<ISaleDiscountService>();
        _eventPublisher = Substitute.For<IDomainEventPublisher>();
        _handler = new UpdateSaleHandler(_saleRepository, _discountService, _eventPublisher);
    }

    [Fact(DisplayName = "UpdateSale with valid data returns result")]
    public async Task Handle_ValidRequest_ReturnsUpdateSaleResult()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var sale = CreateSale(saleId, itemId);
        var command = new UpdateSaleCommand
        {
            Id = saleId,
            SaleDate = DateTime.UtcNow.AddDays(1),
            CustomerId = sale.CustomerId,
            CustomerName = "Updated Customer",
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            Items =
            [
                new UpdateSaleItemDto
                {
                    Id = itemId,
                    ProductId = sale.SaleItems.First().ProductId,
                    ProductName = "Updated Product",
                    Quantity = 5,
                    UnitPrice = 20
                }
            ]
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _discountService.CalculateDiscountPercent(5).Returns(0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(saleId);
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "UpdateSale with cancelled sale throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
    {
        var saleId = Guid.NewGuid();
        var sale = CreateSale(saleId, Guid.NewGuid());
        sale.IsCancelled = true;
        var command = new UpdateSaleCommand
        {
            Id = saleId,
            SaleDate = DateTime.UtcNow,
            CustomerId = sale.CustomerId,
            CustomerName = sale.CustomerName,
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            Items = [
                new UpdateSaleItemDto()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 1,
                    UnitPrice = 10
                }
            ]
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update a cancelled sale.");
    }

    [Fact(DisplayName = "UpdateSale with non-existing sale throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var command = new UpdateSaleCommand
        {
            Id = saleId,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = [
                new UpdateSaleItemDto()
                {
                    Id = Guid.NewGuid(),    
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 1,
                    UnitPrice = 10
                }
            ]
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {saleId} not found");
    }

    private static Sale CreateSale(Guid saleId, Guid itemId)
    {
        return new Sale
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
            SaleItems =
            [
                new SaleItem
                {
                    Id = itemId,
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 10,
                    UnitPrice = 10,
                    DiscountPercent = 0,
                    LineTotal = 100,
                    IsCancelled = false
                }
            ]
        };
    }
}
