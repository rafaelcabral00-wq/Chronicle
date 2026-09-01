namespace Chronicle.Domain.PackTotem;

/// <summary>
/// Captures the unresolved state of the A-012 Totem XP cost contradiction
/// (source Line 1633 says 2 XP, source Line 2820 says 3 XP).
/// <para>
/// E2 deliberately preserves the contradiction: the aggregate does not
/// pick either value. The <see cref="Unresolved"/> state is the only
/// state a freshly-bound Pack may have; explicit resolutions are
/// produced by a future Chronicle-side decision pipeline that this
/// wave does not implement.
/// </para>
/// </summary>
public enum TotemXpResolutionState
{
    Unresolved = 0,
    TwoXp = 2,
    ThreeXp = 3
}
