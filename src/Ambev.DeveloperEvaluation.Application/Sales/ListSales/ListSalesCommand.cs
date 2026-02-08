using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Command for listing sales with pagination.
/// </summary>
public record ListSalesCommand(int Page = 1, int PageSize = 10) : IRequest<ListSalesResult>;
