namespace Chronicle.Domain.PackTotem;

/// <summary>
/// Provider-neutral snapshot of a <see cref="PackTotemAggregate"/>'s
/// persistent state. Captured via
/// <see cref="PackTotemAggregate.CaptureState"/> and rehydrated via
/// <see cref="PackTotemAggregate.Rehydrate"/>. The Application layer
/// is responsible for serialising this record to a
/// <c>Document.PayloadJson</c>; Domain has no JSON attributes.
/// </summary>
public sealed record PackTotemState(
    string PackId,
    string PackName,
    IReadOnlyList<string> Members,
    string? LeaderId,
    string? TotemId,
    int TotemRating,
    IReadOnlyList<string> TotemImprovementPurchases,
    PackTotemLinkState LinkState,
    IReadOnlyList<string> ActiveTactics,
    TotemXpResolutionState LastTotemXpResolution,
    DateTimeOffset? EstablishedAt,
    DateTimeOffset? DissolvedAt);
