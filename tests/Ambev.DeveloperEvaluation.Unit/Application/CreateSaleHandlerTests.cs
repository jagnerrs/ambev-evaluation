using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for CreateSaleHandler.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _discountService = Substitute.For<ISaleDiscountService>();
        _eventPublisher = Substitute.For<IDomainEventPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _discountService, _eventPublisher);
    }

    [Fact(DisplayName = "Valid create request should return sale result")]
    public async Task Handle_ValidRequest_ReturnsCreateSaleResult()
    {
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 2, UnitPrice = 10 }
            ]
        };

        _discountService.CalculateDiscountPercent(2).Returns(0m);
        _saleRepository.GetNextSaleNumberAsync(Arg.Any<CancellationToken>()).Returns("SALE-20250001");

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-20250001"
        };
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be("SALE-20250001");
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Invalid request should throw ValidationException")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateSaleCommand { Items = [] };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Quantity above 20 should throw ValidationException")]
    public async Task Handle_QuantityAbove20_ThrowsValidationException()
    {
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 21, UnitPrice = 10 }
            ]
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
