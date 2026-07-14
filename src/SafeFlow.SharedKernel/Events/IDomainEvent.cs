namespace SafeFlow.SharedKernel.Events;

/// <summary>
/// Marker interface for all domain events in the SafeFlow system.
/// Domain events represent something that has happened in the domain — they are
/// immutable facts raised by aggregate roots and dispatched after persistence.
/// </summary>
/// <remarks>
/// Implementations must be immutable records. The event is identified by the
/// <see cref="OccurredAt"/> timestamp and its own concrete type.
/// No MediatR or other infrastructure dependency is introduced here; the Application
/// layer is responsible for bridging domain events to MediatR notifications.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>Gets the UTC instant at which this domain event occurred.</summary>
    DateTime OccurredAt { get; }
}
