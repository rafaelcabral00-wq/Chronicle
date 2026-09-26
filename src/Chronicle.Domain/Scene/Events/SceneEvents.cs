namespace Chronicle.Domain.Scene.Events;

/// <summary>
/// Emitted when a new Scene is created. Carries the persistent identity,
/// the Session it belongs to, and the name of the new Scene.
/// </summary>
public sealed record SceneCreatedEvent(
    string SceneId,
    string SessionId,
    string SceneName) : IDomainEvent;

/// <summary>
/// Emitted when a Scene is closed.
/// </summary>
public sealed record SceneClosedEvent(
    string SceneId) : IDomainEvent;