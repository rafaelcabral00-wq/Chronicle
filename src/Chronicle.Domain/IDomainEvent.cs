namespace Chronicle.Domain;

/// <summary>
/// Marker for facts that happened inside the Chronicle domain.
/// <para>
/// Domain events are recorded by aggregates as part of state mutation.
/// The aggregate stores them as uncommitted events until the Application
/// layer (added in a later wave) persists the aggregate and dispatches them.
/// </para>
/// <para>
/// E0 deliberately does not define metadata (timestamps, IDs, transport,
/// persistence) so that later waves can introduce such concerns in the
/// correct layer (Application / Infrastructure) without breaking Domain
/// purity.
/// </para>
/// </summary>
public interface IDomainEvent
{
}
