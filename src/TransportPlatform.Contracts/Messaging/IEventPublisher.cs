namespace TransportPlatform.Contracts.Messaging;

/// <summary>
/// Abstraction for publishing integration events to the message broker.
/// Lives in Contracts so Application layer can depend on it without
/// referencing Infrastructure directly.
///
/// Real implementation: MassTransitEventPublisher in Infrastructure.Common
/// Demo implementation: EventPublisherStub in each service's Api project
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;
}
