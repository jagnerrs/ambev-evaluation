using Ambev.DeveloperEvaluation.Application.Events;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for LoggingDomainEventPublisher.
/// </summary>
public class LoggingDomainEventPublisherTests
{
    [Fact(DisplayName = "PublishAsync should complete without throwing")]
    public async Task PublishAsync_AnyEvent_CompletesSuccessfully()
    {
        var publisher = new LoggingDomainEventPublisher();

        var act = () => publisher.PublishAsync(new { EventType = "Test" });

        await act.Should().NotThrowAsync();
    }
}
