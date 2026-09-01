namespace Chronicle.Domain.PackTotem.Events;

/// <summary>
/// Emitted when a Pack is dissolved. After dissolution the aggregate
/// rejects further mutations.
/// </summary>
public sealed record PackDissolvedEvent(string PackId) : IDomainEvent;
