using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for ListSalesHandler.
/// </summary>
public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new ListSalesHandler(_saleRepository);
    }

    [Fact(DisplayName = "ListSales returns paginated result")]
    public async Task Handle_ValidRequest_ReturnsListSalesResult()
    {
        var command = new ListSalesCommand(1, 10);
        var sales = new List<Sale>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-20250001",
                SaleDate = DateTime.UtcNow,
                CustomerName = "Customer",
                BranchName = "Branch",
                TotalAmount = 100,
                IsCancelled = false
            }
        };

        _saleRepository.GetAllAsync(1, 10, Arg.Any<CancellationToken>()).Returns((sales, 1));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "ListSales with invalid page throws ValidationException")]
    public async Task Handle_InvalidPage_ThrowsValidationException()
    {
        var command = new ListSalesCommand(0, 10);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
