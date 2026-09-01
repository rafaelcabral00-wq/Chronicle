namespace Chronicle.Domain;

/// <summary>
/// Convention base class for Chronicle domain aggregates.
/// <para>
/// Implements the uncommitted-event queue and the protected
/// <see cref="RecordEvent"/> method that aggregate implementations
/// call while mutating state. The queue is private; external code
/// can only read or drain it through <see cref="IAggregateRoot"/>.
/// </para>
/// <para>
/// E0 does not introduce snapshots, replay, or commit semantics.
/// After a successful persistence call, the Application layer is
/// expected to call <see cref="DequeueUncommittedEvents"/> to mark
/// the events as committed.
/// </para>
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IDomainEvent> uncommittedEvents = new();

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> UncommittedEvents =>
        uncommittedEvents.ToArray();

    /// <summary>
    /// Constructs an aggregate with a fresh identity.
    /// </summary>
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Constructs an aggregate with a specific identity. Reserved for
    /// rehydration in a future persistence wave; not used by E0.
    /// </summary>
    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Records a domain event as a consequence of state mutation.
    /// Only callable from within aggregate implementations.
    /// </summary>
    protected void RecordEvent(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        uncommittedEvents.Add(@event);
    }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DequeueUncommittedEvents()
    {
        if (uncommittedEvents.Count == 0)
        {
            return Array.Empty<IDomainEvent>();
        }

        var drained = uncommittedEvents.ToArray();
        uncommittedEvents.Clear();
        return drained;
    }
}
