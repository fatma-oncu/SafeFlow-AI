using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Infrastructure.Persistence;

/// <summary>
/// Contract for dispatching domain events collected from aggregate roots.
/// </summary>
/// <remarks>
/// Decouples the <see cref="SafeFlowDbContext"/> from MediatR so the context
/// does not take a direct dependency on <c>IPublisher</c> and can be tested
/// with a simple in-memory dispatcher.
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events collected from the given aggregates.
    /// </summary>
    /// <param name="events">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task DispatchAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}
