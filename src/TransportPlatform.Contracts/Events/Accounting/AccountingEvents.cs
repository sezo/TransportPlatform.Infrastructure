namespace TransportPlatform.Contracts.Events.Accounting;

/// <summary>
/// Published by: Accounting service (saga response to TicketReserved)
/// Consumed by:  Ticketing saga (proceed to capacity check)
/// </summary>
public record PaymentProcessed(
    Guid TicketId,
    Guid InvoiceId,
    decimal Amount,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Accounting service (payment declined or error)
/// Consumed by:  Ticketing saga (compensation — cancel reservation)
/// </summary>
public record PaymentFailed(
    Guid TicketId,
    string Reason,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Accounting service (after fiscalization)
/// Consumed by:  Reporting, Ticketing (attach fiscal number to ticket)
/// </summary>
public record InvoiceFiscalized(
    Guid TicketId,
    Guid InvoiceId,
    string FiscalNumber,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by: Accounting service (refund completed)
/// Consumed by:  Reporting, Ticketing (confirm cancellation)
/// </summary>
public record PaymentRefunded(
    Guid TicketId,
    Guid InvoiceId,
    decimal Amount,
    DateTimeOffset OccurredAt);
