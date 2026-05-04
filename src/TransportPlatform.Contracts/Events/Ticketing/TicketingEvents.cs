namespace TransportPlatform.Contracts.Events.Ticketing;

/// <summary>
/// Published by: Ticketing service
/// Consumed by:  Accounting (process payment), Vehicle (reserve capacity), Reporting
/// </summary>
public record TicketReserved(
    Guid TicketId,
    Guid UserId,
    Guid RouteId,
    int SeatNumber,
    decimal Price,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Ticketing service (after payment + capacity confirmed)
/// Consumed by:  Reporting, Notification service (future)
/// </summary>
public record TicketConfirmed(
    Guid TicketId,
    Guid UserId,
    Guid RouteId,
    string RouteName,
    string RouteOrigin,
    string RouteDestination,
    int SeatNumber,
    decimal Price,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Ticketing service (saga compensation or manual cancel)
/// Consumed by:  Accounting (refund), Vehicle (release capacity), Reporting
/// </summary>
public record TicketCancelled(
    Guid TicketId,
    Guid UserId,
    string Reason,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Ticketing service (inspector validates ticket)
/// Consumed by:  Vehicle (passenger count), Reporting
/// </summary>
public record TicketValidated(
    Guid TicketId,
    Guid UserId,
    Guid RouteId,
    Guid InspectorId,
    DateTimeOffset OccurredAt);
