using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Contracts.Messaging;

namespace TransportPlatform.Infrastructure.Common.Messaging;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers MassTransit + RabbitMQ with shared connection config.
    /// Each service passes its own consumers and sagas via the configure action.
    ///
    /// Usage in each service Program.cs:
    ///   builder.Services.AddTransportMessaging(builder.Configuration, x =>
    ///   {
    ///       x.AddConsumer&lt;PaymentProcessedConsumer&gt;();
    ///       x.AddSagaStateMachine&lt;TicketPurchaseSaga, TicketPurchaseSagaState&gt;();
    ///   });
    /// </summary>
    public static IServiceCollection AddTransportMessaging(
        this IServiceCollection services,
        IConfiguration config,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(x =>
        {
            // Each service registers its own consumers and sagas
            configure?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(config["RabbitMQ:Host"] ?? "rabbitmq", h =>
                {
                    h.Username(config["RabbitMQ:Username"] ?? "transport");
                    h.Password(config["RabbitMQ:Password"] ?? "transport");
                });

                // Auto-creates queues and bindings for all registered consumers
                cfg.ConfigureEndpoints(ctx);
            });
        });

        // Register publisher — services inject IEventPublisher
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}
