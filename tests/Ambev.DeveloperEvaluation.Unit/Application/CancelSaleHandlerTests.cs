using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for CancelSaleHandler.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IDomainEventPublisher>();
        _handler = new CancelSaleHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "CancelSale with existing sale returns success")]
    public async Task Handle_ExistingSale_ReturnsSuccess()
    {
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand(saleId);
        var sale = CreateSale(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "CancelSale with non-existing sale throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {saleId} not found");
    }

    [Fact(DisplayName = "CancelSale with invalid Id throws ValidationException")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var command = new CancelSaleCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static Sale CreateSale(Guid saleId)
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
            SaleItems = []
        };
    }
}
