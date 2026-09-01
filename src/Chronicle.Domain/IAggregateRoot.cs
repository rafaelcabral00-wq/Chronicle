namespace Chronicle.Domain;

/// <summary>
/// Root of a Chronicle domain aggregate.
/// <para>
/// An aggregate is a consistency boundary around a cluster of domain
/// objects. It owns its state, mutates state only through its own
/// behaviour, and records what happened as <see cref="IDomainEvent"/>s.
/// </para>
/// <para>
/// External code can observe the aggregate's identity and drain its
/// uncommitted events; it cannot mutate the event collection or the
/// aggregate state directly. Recording events is a privilege reserved
/// to aggregate implementations (see <see cref="AggregateRoot"/>).
/// </para>
/// <para>
/// E0 deliberately exposes only the minimum surface required to
/// support a future Pack/Totem aggregate: stable identity and read-only
/// access to uncommitted events. Persistence, repositories, and
/// serialisation concerns are intentionally absent and belong in
/// later waves.
/// </para>
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Stable identity of this aggregate.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Read-only snapshot of domain events that have been recorded
    /// since the last commit. Returns a new read-only collection;
    /// the underlying queue is not exposed.
    /// </summary>
    IReadOnlyList<IDomainEvent> UncommittedEvents { get; }

    /// <summary>
    /// Removes all currently uncommitted events and returns them
    /// to the caller. Called by the Application layer after the
    /// aggregate has been persisted.
    /// </summary>
    IReadOnlyList<IDomainEvent> DequeueUncommittedEvents();
}
