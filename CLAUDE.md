# TransportPlatform.Contracts — Claude context

## Purpose
Two NuGet packages shared across all TransportPlatform microservices.
This repo is the ONLY shared code between services — everything else is service-owned.

## Packages

### TransportPlatform.Contracts
Zero dependencies. Pure C# records.
- Integration events (what happened — immutable facts)
- IEventPublisher interface (how to publish events)

### TransportPlatform.Infrastructure.Common
Shared infrastructure setup. Every service calls these extensions in Program.cs.
- AddTransportAuth() — JWT validation, UserContext, PermissionPolicyProvider
- AddTransportMessaging() — MassTransit + RabbitMQ, MassTransitEventPublisher
- AddTransportObservability() — OTel tracing + metrics → Grafana
- AddTransportLogging() — Serilog → OTel → Loki

## Versioning policy
- NEVER remove or rename fields from events — consumers will break
- NEVER rename event classes — consumers will break
- Adding nullable fields is backwards compatible
- Bump minor version for additions: 1.0.0 → 1.1.0
- Bump major version for breaking changes: 1.0.0 → 2.0.0
- Breaking changes require coordinated deployment of all consumers

## Publishing
Packages published to internal BaGet on merge to main.
Pipeline: CI (build + pack verify) → CD (pack + push to BaGet)
Services reference via NuGet.config pointing to http://nuget.internal

## PR policy
Changes to this repo require approval from ALL team leads.
A breaking event change affects every service — coordinate before merging.

## What NOT to put here
- Business logic of any kind
- Domain entities
- EF Core DbContext
- Service-specific code
- Anything that only one service needs
