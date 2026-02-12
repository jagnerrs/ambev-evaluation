using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for DeleteSaleHandler.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "DeleteSale with existing sale returns success")]
    public async Task Handle_ExistingSale_ReturnsSuccess()
    {
        var saleId = Guid.NewGuid();
        var command = new DeleteSaleCommand(saleId);

        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _saleRepository.Received(1).DeleteAsync(saleId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "DeleteSale with non-existing sale throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var command = new DeleteSaleCommand(saleId);

        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {saleId} not found");
    }

    [Fact(DisplayName = "DeleteSale with invalid Id throws ValidationException")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var command = new DeleteSaleCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
