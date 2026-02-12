using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for CancelSaleItemCommand.
/// When partially cancelling, recalculates the discount based on the remaining quantity.
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        ISaleDiscountService discountService,
        IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _discountService = discountService;
        _eventPublisher = eventPublisher;
    }

    public async Task<CancelSaleItemResponse> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var item = sale.SaleItems.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID {request.ItemId} not found in sale");

        if (item.IsCancelled)
            throw new InvalidOperationException("Item is already cancelled");

        var quantityToCancel = request.QuantityToCancel.HasValue
            ? Math.Min(request.QuantityToCancel.Value, item.Quantity)
            : item.Quantity;
        var isPartialCancel = quantityToCancel > 0 && quantityToCancel < item.Quantity;

        if (isPartialCancel)
        {
            var newQuantity = item.Quantity - quantityToCancel;
            var discountPercent = _discountService.CalculateDiscountPercent(newQuantity);
            var subtotal = newQuantity * item.UnitPrice;
            var discountAmount = subtotal * (discountPercent / 100);
            var lineTotal = subtotal - discountAmount;

            sale.ReduceItemQuantity(request.ItemId, newQuantity, discountPercent, lineTotal);
        }
        else
        {
            sale.CancelItem(request.ItemId);
        }

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var updatedItem = sale.SaleItems.First(i => i.Id == request.ItemId);
        await _eventPublisher.PublishAsync(new ItemCancelledEvent(sale, updatedItem), cancellationToken);

        return new CancelSaleItemResponse { Success = true };
    }
}
