namespace Chronicle.Domain.Session;

/// <summary>
/// Provider-neutral snapshot of a <see cref="SessionAggregate"/>'s
/// persistent state. Captured via
/// <see cref="SessionAggregate.CaptureState"/> and rehydrated via
/// <see cref="SessionAggregate.Rehydrate"/>. The Application layer
/// is responsible for serialising this record to a
/// <c>Document.PayloadJson</c>; Domain has no JSON attributes.
/// </summary>
public sealed record SessionState(
    string SessionId,
    string CampaignId,
    string SessionName,
    SessionLifecycle Lifecycle,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);