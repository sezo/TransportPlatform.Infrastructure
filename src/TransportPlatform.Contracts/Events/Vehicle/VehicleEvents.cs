namespace TransportPlatform.Contracts.Events.Vehicle;

/// <summary>
/// Published by: Vehicle service (onboard CPU GPS ping)
/// Consumed by:  Reporting (live tracking), Simulation verification
/// </summary>
public record VehiclePositionUpdated(
    Guid VehicleId,
    Guid RouteId,
    double Latitude,
    double Longitude,
    double SpeedKmh,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Vehicle service (saga response to TicketReserved)
/// Consumed by:  Ticketing saga (proceed to payment)
/// </summary>
public record CapacityReserved(
    Guid TicketId,
    Guid VehicleId,
    int SeatNumber,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Vehicle service (no capacity available)
/// Consumed by:  Ticketing saga (compensation — cancel + refund)
/// </summary>
public record CapacityReservationFailed(
    Guid TicketId,
    string Reason,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Vehicle service (driver starts route)
/// Consumed by:  Reporting, Ticketing (activate tickets for route)
/// </summary>
public record RouteStarted(
    Guid RouteId,
    Guid VehicleId,
    Guid DriverId,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Vehicle service (driver ends route)
/// Consumed by:  Reporting, Accounting (finalize billing)
/// </summary>
public record RouteCompleted(
    Guid RouteId,
    Guid VehicleId,
    int PassengerCount,
    DateTimeOffset OccurredAt);
