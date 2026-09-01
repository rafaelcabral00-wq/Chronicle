namespace Chronicle.Domain.PackTotem;

/// <summary>
/// Lifecycle state of the Pack ↔ Totem linkage.
/// <list type="bullet">
/// <item><see cref="Unbound"/>: Pack has no Totem.</item>
/// <item><see cref="Bound"/>: Pack has a bound Totem.</item>
/// <item><see cref="Dissolving"/>: Pack is in the process of dissolving
/// (transitional state per source Line 199, where dissolution releases
/// the Totem via ceremony).</item>
/// </list>
/// </summary>
public enum PackTotemLinkState
{
    Unbound = 0,
    Bound = 1,
    Dissolving = 2
}
