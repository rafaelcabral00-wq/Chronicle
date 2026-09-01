namespace Chronicle.Domain.PackTotem.Events;

/// <summary>
/// Emitted when a Pack binds to a Totem. The <c>TotemRating</c> is the
/// sum of member investments at binding time per source Line 1632.
/// The aggregate remains in <c>Unresolved</c> A-012 state until a
/// future Chronicle pipeline decides the XP cost.
/// </summary>
public sealed record TotemBoundEvent(
    string PackId,
    string TotemId,
    int TotemRating,
    int TotemAggregation) : IDomainEvent;
