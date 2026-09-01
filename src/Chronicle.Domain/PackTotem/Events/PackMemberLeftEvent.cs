namespace Chronicle.Domain.PackTotem.Events;

/// <summary>
/// Emitted when a character leaves a Pack.
/// </summary>
public sealed record PackMemberLeftEvent(
    string PackId,
    string CharacterId) : IDomainEvent;
