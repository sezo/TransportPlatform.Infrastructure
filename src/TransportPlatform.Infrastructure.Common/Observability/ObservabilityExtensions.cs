using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace TransportPlatform.Infrastructure.Common.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing and metrics.
    /// Sends to OTel Collector → Grafana Tempo + Prometheus.
    ///
    /// Usage: builder.Services.AddTransportObservability(builder.Configuration, "transport-ticketing");
    /// </summary>
    public static IServiceCollection AddTransportObservability(
        this IServiceCollection services,
        IConfiguration config,
        string serviceName)
    {
        var otlpEndpoint = config["Observability:OtlpEndpoint"]
            ?? "http://otel-collector:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.EnrichWithHttpRequest = (activity, request) =>
                        activity.SetTag("correlation.id",
                            request.Headers["X-Correlation-Id"]
                                .FirstOrDefault());
                })
                .AddSource("MassTransit")
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }
}
