using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for UpdateSaleCommand.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        ISaleDiscountService discountService,
        IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _discountService = discountService;
        _eventPublisher = eventPublisher;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale.");

        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
        sale.UpdatedAt = DateTime.UtcNow;

        var existingItemIds = command.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
        var itemsToRemove = sale.SaleItems.Where(i => !existingItemIds.Contains(i.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            sale.SaleItems.Remove(item);
        }

        foreach (var itemDto in command.Items)
        {
            var discountPercent = _discountService.CalculateDiscountPercent(itemDto.Quantity);
            var subtotal = itemDto.Quantity * itemDto.UnitPrice;
            var discountAmount = subtotal * (discountPercent / 100);
            var lineTotal = subtotal - discountAmount;

            if (itemDto.Id.HasValue)
            {
                var existingItem = sale.SaleItems.FirstOrDefault(i => i.Id == itemDto.Id.Value);
                if (existingItem != null && !existingItem.IsCancelled)
                {
                    existingItem.ProductId = itemDto.ProductId;
                    existingItem.ProductName = itemDto.ProductName;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.UnitPrice = itemDto.UnitPrice;
                    existingItem.DiscountPercent = discountPercent;
                    existingItem.LineTotal = lineTotal;
                }
            }
            else
            {
                var newItem = new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = sale.Id,
                    ProductId = itemDto.ProductId,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountPercent = discountPercent,
                    LineTotal = lineTotal,
                    IsCancelled = false
                };
                sale.SaleItems.Add(newItem);
            }
        }

        sale.RecalculateTotal();

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleModifiedEvent(updatedSale), cancellationToken);

        return new UpdateSaleResult
        {
            Id = updatedSale.Id,
            SaleNumber = updatedSale.SaleNumber
        };
    }
}
