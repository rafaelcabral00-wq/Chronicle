namespace Chronicle.Domain.Scene;

/// <summary>
/// Provider-neutral snapshot of a <see cref="SceneAggregate"/>'s
/// persistent state. Captured via
/// <see cref="SceneAggregate.CaptureState"/> and rehydrated via
/// <see cref="SceneAggregate.Rehydrate"/>. The Application layer
/// is responsible for serialising this record to a
/// <c>Document.PayloadJson</c>; Domain has no JSON attributes.
/// </summary>
public sealed record SceneState(
    string SceneId,
    string SessionId,
    string SceneName,
    SceneLifecycle Lifecycle,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);