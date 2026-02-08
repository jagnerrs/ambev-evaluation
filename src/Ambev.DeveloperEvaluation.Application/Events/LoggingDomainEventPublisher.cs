using Serilog;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Logs domain events to the application log instead of publishing to a message broker.
/// </summary>
public class LoggingDomainEventPublisher : IDomainEventPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(object domainEvent, CancellationToken cancellationToken = default)
    {
        Log.Information("Domain Event: {EventType} - {Event}", domainEvent.GetType().Name, domainEvent);
        return Task.CompletedTask;
    }
}
