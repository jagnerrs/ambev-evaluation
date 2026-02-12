using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for GetSaleHandler.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new GetSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "GetSale with existing sale returns GetSaleResult")]
    public async Task Handle_ExistingSale_ReturnsGetSaleResult()
    {
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);
        var sale = CreateSale(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(saleId);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GetSale with non-existing sale throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {saleId} not found");
    }

    [Fact(DisplayName = "GetSale with invalid Id throws ValidationException")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var command = new GetSaleCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static Sale CreateSale(Guid saleId)
    {
        var itemId = Guid.NewGuid();
        return new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-20250001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            TotalAmount = 80,
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
                    DiscountPercent = 20,
                    LineTotal = 80,
                    IsCancelled = false
                }
            ]
        };
    }
}
