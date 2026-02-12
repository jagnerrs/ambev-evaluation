using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.SaleItems).OrderByDescending(s => s.SaleDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(sale);
        var keptItemIds = sale.SaleItems.Select(i => i.Id).ToHashSet();

        if (entry.State == EntityState.Detached)
        {
            var itemsToDelete = await _context.SaleItems
                .Where(i => i.SaleId == sale.Id && !keptItemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);
            _context.SaleItems.RemoveRange(itemsToDelete);
            _context.Sales.Update(sale);
        }
        else
        {
            var itemsToDelete = await _context.SaleItems
                .Where(i => i.SaleId == sale.Id && !keptItemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);
            _context.SaleItems.RemoveRange(itemsToDelete);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string> GetNextSaleNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SALE-{year}";

        var lastSale = await _context.Sales
            .Where(s => s.SaleNumber.StartsWith(prefix))
            .OrderByDescending(s => s.SaleNumber)
            .Select(s => s.SaleNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (!string.IsNullOrEmpty(lastSale) && lastSale.Length > prefix.Length)
        {
            var sequencePart = lastSale[prefix.Length..];
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
