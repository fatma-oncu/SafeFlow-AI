using MediatR;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Infrastructure.Persistence;

/// <summary>
/// MediatR-backed implementation of <see cref="IDomainEventDispatcher"/>.
/// </summary>
/// <remarks>
/// <para>
/// Domain events (<see cref="IDomainEvent"/>) are defined in the Domain layer
/// without any MediatR dependency. This adapter wraps each concrete domain event
/// in a generic <see cref="DomainEventNotification{TEvent}"/> notification so that
/// the Application layer can implement <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c>
/// for each event type.
/// </para>
/// </remarks>
internal sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initialises a new <see cref="MediatRDomainEventDispatcher"/>.
    /// </summary>
    public MediatRDomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc/>
    public async Task DispatchAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            // Wrap in the generic notification envelope and publish
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }
    }
}

/// <summary>
/// Generic MediatR notification envelope that wraps a domain event.
/// </summary>
/// <typeparam name="TEvent">The concrete domain event type.</typeparam>
/// <param name="DomainEvent">The domain event being wrapped.</param>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent)
    : INotification
    where TEvent : IDomainEvent;
