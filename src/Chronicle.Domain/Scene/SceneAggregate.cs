using Chronicle.Domain.Scene.Events;

namespace Chronicle.Domain.Scene;

/// <summary>
/// Chronicle Domain aggregate representing a Scene.
/// <para>
/// A Scene belongs to a Session but is an independent aggregate.
/// It stores only the minimum identifiers, lifecycle state, and timestamps.
/// Rule-set-specific mechanics (acts, characters, packs, world state,
/// narrative turns, dice, LLM state) are intentionally not part of this aggregate.
/// </para>
/// <para>
/// The aggregate mutates only through domain methods. Each mutation
/// records exactly one <see cref="IDomainEvent"/> through the inherited
/// <c>RecordEvent</c> mechanism. Mutations after closure throw
/// <see cref="InvalidOperationException"/>; the aggregate does not
/// silently no-op.
/// </para>
/// </summary>
public sealed class SceneAggregate : AggregateRoot
{
    private SceneAggregate()
    {
    }

    private SceneAggregate(string sceneId, string sessionId, string sceneName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            throw new ArgumentException("Scene identifier must not be empty.", nameof(sceneId));
        }
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session identifier must not be empty.", nameof(sessionId));
        }
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("Scene name must not be empty.", nameof(sceneName));
        }

        SceneId = sceneId;
        SessionId = sessionId;
        SceneName = sceneName;
        Lifecycle = SceneLifecycle.Active;
        CreatedAt = createdAt;
        RecordEvent(new SceneCreatedEvent(sceneId, sessionId, sceneName));
    }

    public string SceneId { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public string SceneName { get; private set; } = string.Empty;
    public SceneLifecycle Lifecycle { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>
    /// Creates a new Scene with Active lifecycle.
    /// </summary>
    public static SceneAggregate Create(
        string sceneId,
        string sessionId,
        string sceneName,
        DateTimeOffset createdAt)
    {
        return new SceneAggregate(sceneId, sessionId, sceneName, createdAt);
    }

    /// <summary>
    /// Constructs an aggregate from a previously persisted state. Does
    /// not record any domain event. Used by the Application persistence
    /// layer to rehydrate the aggregate from a <c>Document</c>.
    /// </summary>
    public static SceneAggregate Rehydrate(SceneState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var aggregate = new SceneAggregate();
        aggregate.SceneId = state.SceneId;
        aggregate.SessionId = state.SessionId;
        aggregate.SceneName = state.SceneName;
        aggregate.Lifecycle = state.Lifecycle;
        aggregate.CreatedAt = state.CreatedAt;
        aggregate.ClosedAt = state.ClosedAt;
        return aggregate;
    }

    /// <summary>
    /// Captures the aggregate's current state for persistence. Does not
    /// mutate or record events.
    /// </summary>
    public SceneState CaptureState() => new(
        SceneId,
        SessionId,
        SceneName,
        Lifecycle,
        CreatedAt,
        ClosedAt);

    /// <summary>
    /// Closes the Scene. After closure, the aggregate rejects all
    /// further mutations.
    /// </summary>
    public void Close(DateTimeOffset closedAt)
    {
        EnsureNotClosed();
        Lifecycle = SceneLifecycle.Closed;
        ClosedAt = closedAt;
        RecordEvent(new SceneClosedEvent(SceneId));
    }

    private void EnsureNotClosed()
    {
        if (Lifecycle == SceneLifecycle.Closed)
        {
            throw new InvalidOperationException(
                $"Scene '{SceneId}' is closed; mutations are not permitted.");
        }
    }
}