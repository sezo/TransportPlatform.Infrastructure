using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace TransportPlatform.Infrastructure.Common.Observability;

public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog with console + OpenTelemetry sink.
    /// Sends structured logs to OTel Collector → Grafana Loki.
    ///
    /// Usage: builder.Host.AddTransportLogging(builder.Configuration, "transport-ticketing");
    /// </summary>
    public static IHostBuilder AddTransportLogging(
        this IHostBuilder host,
        IConfiguration config,
        string serviceName)
    {
        return host.UseSerilog((ctx, services, loggerConfig) =>
            loggerConfig
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(otel =>
                {
                    otel.Endpoint = config["Observability:OtlpEndpoint"]
                        ?? "http://otel-collector:4317";
                    otel.Protocol = OtlpProtocol.Grpc;
                    otel.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName
                    };
                }));
    }
}
