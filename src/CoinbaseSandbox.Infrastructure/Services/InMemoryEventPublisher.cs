namespace CoinbaseSandbox.Infrastructure.Services;

using System.Collections.Concurrent;
using CoinbaseSandbox.Application.Services;
using Domain.Events;
using Microsoft.Extensions.Logging;

public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    // Store of subscribers - in a real implementation this would be more sophisticated
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event {EventType} with ID {EventId}",
            eventType.Name, @event.Id);

        // In this sandbox implementation, we just log the event
        // In a real system, we would notify subscribers

        return Task.CompletedTask;
    }

    // Method to register handlers (for demo purposes)
    public void RegisterHandler<TEvent>(Action<TEvent> handler)
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);

        _handlers.AddOrUpdate(
            eventType,
            new List<Delegate> { handler },
            (_, existing) =>
            {
                existing.Add(handler);
                return existing;
            });
    }
}