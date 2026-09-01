namespace Chronicle.Domain.PackTotem.Events;

/// <summary>
/// Emitted when a new Pack is created. Carries the persistent identity
/// and the name of the new Pack; no Werewolf-typed fields.
/// </summary>
public sealed record PackCreatedEvent(
    string PackId,
    string PackName) : IDomainEvent;
