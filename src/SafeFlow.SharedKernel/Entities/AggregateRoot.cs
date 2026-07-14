using SafeFlow.SharedKernel.Events;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.SharedKernel.Entities;

/// <summary>
/// Base class for all aggregate roots in the SafeFlow domain model.
/// </summary>
/// <remarks>
/// <para>
/// An <em>aggregate root</em> is the only member of its aggregate cluster that
/// external objects are permitted to hold references to. It enforces all invariants
/// of the cluster and is the single unit of transactional consistency.
/// </para>
/// <para>
/// This class extends <see cref="BaseEntity"/> (which provides <see cref="Guid"/>
/// identity, audit fields, and soft-deletion) and adds internal domain event
/// management. Domain events collected here are dispatched by the Infrastructure
/// layer's Unit of Work after <c>SaveChangesAsync</c> succeeds, guaranteeing
/// that events are only published when the aggregate's state change is durably
/// persisted.
/// </para>
/// <para>
/// The <see cref="IDomainEvent"/> interface is intentionally infrastructure-agnostic.
/// The Application layer is responsible for adapting each domain event to a MediatR
/// <c>INotification</c>.
/// </para>
/// </remarks>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the ordered collection of domain events raised during the current
    /// unit of work. This collection is read-only from the outside; events are
    /// added via <see cref="RaiseDomainEvent"/> and cleared via
    /// <see cref="ClearDomainEvents"/>.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event to be dispatched after the aggregate's state change
    /// is successfully persisted.
    /// </summary>
    /// <param name="domainEvent">
    /// The domain event to raise. Must not be <c>null</c>.
    /// </param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all accumulated domain events. Called by the Infrastructure layer
    /// after the events have been successfully dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
