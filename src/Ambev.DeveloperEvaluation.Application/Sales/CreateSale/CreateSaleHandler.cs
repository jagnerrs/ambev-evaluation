using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for CreateSaleCommand.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleDiscountService _discountService;
    private readonly IDomainEventPublisher _eventPublisher;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleDiscountService discountService,
        IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _discountService = discountService;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var saleNumber = await _saleRepository.GetNextSaleNumberAsync(cancellationToken);

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in command.Items)
        {
            var discountPercent = _discountService.CalculateDiscountPercent(itemDto.Quantity);
            var subtotal = itemDto.Quantity * itemDto.UnitPrice;
            var discountAmount = subtotal * (discountPercent / 100);
            var lineTotal = subtotal - discountAmount;

            var item = new SaleItem
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
            sale.SaleItems.Add(item);
        }

        sale.RecalculateTotal();

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCreatedEvent(createdSale), cancellationToken);

        return new CreateSaleResult
        {
            Id = createdSale.Id,
            SaleNumber = createdSale.SaleNumber
        };
    }
}
