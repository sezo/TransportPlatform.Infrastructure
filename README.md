# TransportPlatform.Contracts

Shared NuGet packages for TransportPlatform microservices.

## Packages

| Package | Version | Purpose |
|---|---|---|
| `TransportPlatform.Contracts` | 1.0.0 | Integration events, IEventPublisher |
| `TransportPlatform.Infrastructure.Common` | 1.0.0 | Auth, messaging, observability, logging |

## Installing in a service

Add `NuGet.config` to your service repo (copy from this repo), then:

```bash
dotnet add package TransportPlatform.Contracts
dotnet add package TransportPlatform.Infrastructure.Common
```

## Usage in service Program.cs

```csharp
// Logging
builder.Host.AddTransportLogging(builder.Configuration, "transport-ticketing");

// Observability
builder.Services.AddTransportObservability(builder.Configuration, "transport-ticketing");

// Auth (JWT validation + UserContext + PermissionPolicyProvider)
builder.Services.AddTransportAuth(builder.Configuration);

// Messaging (MassTransit + RabbitMQ)
builder.Services.AddTransportMessaging(builder.Configuration, x =>
{
    x.AddConsumer<PaymentProcessedConsumer>();
});

// Middleware pipeline
app.UseAuthentication();
app.UseTransportUserContext();   // reads X-User-* headers from gateway
app.UseAuthorization();
```

## Local development (BaGet)

Run BaGet locally:
```bash
docker run -d -p 5555:80 --name baget loicsharma/baget
```

Push packages manually:
```bash
dotnet pack --configuration Release
dotnet nuget push **/*.nupkg --source http://localhost:5555/v3/index.json --api-key local
```

## Versioning

Backwards compatible (minor bump):
- Adding nullable fields to events
- Adding new event classes
- Adding new extension methods

Breaking change (major bump — coordinate with all teams):
- Removing or renaming event fields
- Renaming event classes
- Changing method signatures in extensions
