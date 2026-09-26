namespace Chronicle.Domain.Session.Events;

/// <summary>
/// Emitted when a new Session is created. Carries the persistent identity,
/// the Campaign it belongs to, and the name of the new Session.
/// </summary>
public sealed record SessionCreatedEvent(
    string SessionId,
    string CampaignId,
    string SessionName) : IDomainEvent;

/// <summary>
/// Emitted when a Session is closed.
/// </summary>
public sealed record SessionClosedEvent(
    string SessionId) : IDomainEvent;