using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale entity operations.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the repository.
    /// </summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its unique identifier including items.
    /// </summary>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its sale number including items.
    /// </summary>
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves sales with pagination.
    /// </summary>
    Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sale.
    /// </summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale from the repository.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next sequential sale number for the current year.
    /// </summary>
    Task<string> GetNextSaleNumberAsync(CancellationToken cancellationToken = default);
}
