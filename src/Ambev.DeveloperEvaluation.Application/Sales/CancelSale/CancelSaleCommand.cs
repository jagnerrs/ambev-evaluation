using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Command for cancelling a sale.
/// </summary>
public record CancelSaleCommand(Guid Id) : IRequest<CancelSaleResponse>;
