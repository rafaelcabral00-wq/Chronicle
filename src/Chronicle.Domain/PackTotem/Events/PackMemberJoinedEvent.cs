namespace Chronicle.Domain.PackTotem.Events;

/// <summary>
/// Emitted when a character joins a Pack. The character identity is
/// opaque to Chronicle Domain (a string reference supplied by the
/// application/character layer).
/// </summary>
public sealed record PackMemberJoinedEvent(
    string PackId,
    string CharacterId) : IDomainEvent;
