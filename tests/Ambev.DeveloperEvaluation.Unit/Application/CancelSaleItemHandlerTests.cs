using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for CancelSaleItemHandler.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _discountService = Substitute.For<ISaleDiscountService>();
        _eventPublisher = Substitute.For<IDomainEventPublisher>();
        _handler = new CancelSaleItemHandler(_saleRepository, _discountService, _eventPublisher);
    }

    [Fact(DisplayName = "CancelSaleItem full cancel returns success")]
    public async Task Handle_FullCancel_ReturnsSuccess()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CancelSaleItemCommand(saleId, itemId);
        var sale = CreateSaleWithItem(saleId, itemId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "CancelSaleItem partial cancel recalculates discount")]
    public async Task Handle_PartialCancel_RecalculatesDiscount()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CancelSaleItemCommand(saleId, itemId, 5);
        var sale = CreateSaleWithItem(saleId, itemId, quantity: 10);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _discountService.CalculateDiscountPercent(5).Returns(0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _discountService.Received(1).CalculateDiscountPercent(5);
    }

    [Fact(DisplayName = "CancelSaleItem with non-existing sale throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        var command = new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid());

        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "CancelSaleItem with already cancelled item throws InvalidOperationException")]
    public async Task Handle_AlreadyCancelledItem_ThrowsInvalidOperationException()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CancelSaleItemCommand(saleId, itemId);
        var sale = CreateSaleWithItem(saleId, itemId);
        sale.SaleItems.First().IsCancelled = true;

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Item is already cancelled");
    }

    private static Sale CreateSaleWithItem(Guid saleId, Guid itemId, int quantity = 10)
    {
        var item = new SaleItem
        {
            Id = itemId,
            SaleId = saleId,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            Quantity = quantity,
            UnitPrice = 10,
            DiscountPercent = 0,
            LineTotal = quantity * 10,
            IsCancelled = false
        };
        return new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-20250001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            TotalAmount = item.LineTotal,
            IsCancelled = false,
            SaleItems = [item]
        };
    }
}
