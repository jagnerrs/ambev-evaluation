using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling a specific item within a sale.
/// When QuantityToCancel is provided and less than item quantity, performs partial cancellation
/// and recalculates the discount based on the remaining quantity.
/// </summary>
public record CancelSaleItemCommand(Guid SaleId, Guid ItemId, int? QuantityToCancel = null) : IRequest<CancelSaleItemResponse>;
