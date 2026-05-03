using MassTransit;
using TransportPlatform.Contracts.Messaging;

namespace TransportPlatform.Infrastructure.Common.Messaging;

/// <summary>
/// Real MassTransit implementation of IEventPublisher.
/// Registered via AddTransportMessaging() extension.
/// Replaces EventPublisherStub in each service when Infrastructure is wired up.
/// </summary>
public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        await publishEndpoint.Publish(@event, ct);
    }
}
