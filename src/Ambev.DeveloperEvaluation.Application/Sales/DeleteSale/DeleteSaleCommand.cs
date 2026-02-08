using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Command for deleting a sale.
/// </summary>
public record DeleteSaleCommand(Guid Id) : IRequest<DeleteSaleResponse>;
