namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Publishes domain events. Implementations may log or forward to a message broker.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event (e.g., logs to application log).
    /// </summary>
    Task PublishAsync(object domainEvent, CancellationToken cancellationToken = default);
}
