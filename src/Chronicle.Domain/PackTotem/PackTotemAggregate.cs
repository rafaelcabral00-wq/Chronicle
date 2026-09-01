using Chronicle.Domain.PackTotem.Events;

namespace Chronicle.Domain.PackTotem;

/// <summary>
/// The first concrete Chronicle Domain aggregate: a persistent
/// Pack ↔ Totem relationship.
/// <para>
/// E2 implements the minimum state required by the Pack/Totem audit
/// (AUDIT-PACK-TOTEM-AGGREGATE-RUNTIME-2026-08-31.md §6.1) without
/// inventing Werewolf mechanics. The aggregate is persistence-agnostic,
/// narrative-agnostic, and Werewolf-agnostic; it stores only the
/// minimum identifiers, the linkage state, and the binding rating.
/// </para>
/// <para>
/// State ownership:
/// <list type="bullet">
/// <item>Pack identity, name, members, leader, dissolution state —
///   Chronicle Domain (this aggregate).</item>
/// <item>Totem identity, rating, improvement purchases, link state,
///   A-012 preservation — Chronicle Domain (this aggregate).</item>
/// <item>Totem catalog, effect catalog, deterministic validation,
///   ritual mechanics — Werewolf rule set (untouched by E2).</item>
/// </list>
/// </para>
/// <para>
/// The aggregate mutates only through domain methods. Each mutation
/// records exactly one <see cref="IDomainEvent"/> through the inherited
/// <c>RecordEvent</c> mechanism. Mutations after dissolution throw
/// <see cref="InvalidOperationException"/>; the aggregate does not
/// silently no-op.
/// </para>
/// </summary>
public sealed class PackTotemAggregate : AggregateRoot
{
    private readonly List<string> members = new();
    private readonly List<string> totemImprovementPurchases = new();
    private readonly List<string> activeTactics = new();

    public string PackId { get; private set; } = string.Empty;
    public string PackName { get; private set; } = string.Empty;
    public IReadOnlyList<string> Members => members.ToArray();
    public string? LeaderId { get; private set; }
    public string? TotemId { get; private set; }
    public int TotemRating { get; private set; }
    public IReadOnlyList<string> TotemImprovementPurchases => totemImprovementPurchases.ToArray();
    public PackTotemLinkState LinkState { get; private set; } = PackTotemLinkState.Unbound;
    public IReadOnlyList<string> ActiveTactics => activeTactics.ToArray();
    public TotemXpResolutionState LastTotemXpResolution { get; private set; } = TotemXpResolutionState.Unresolved;
    public DateTimeOffset? EstablishedAt { get; private set; }
    public DateTimeOffset? DissolvedAt { get; private set; }
    public bool IsDissolved => DissolvedAt is not null;

    private PackTotemAggregate()
    {
    }

    private PackTotemAggregate(string packId, string packName, DateTimeOffset establishedAt)
    {
        if (string.IsNullOrWhiteSpace(packId))
        {
            throw new ArgumentException("Pack identifier must not be empty.", nameof(packId));
        }
        if (string.IsNullOrWhiteSpace(packName))
        {
            throw new ArgumentException("Pack name must not be empty.", nameof(packName));
        }

        PackId = packId;
        PackName = packName;
        EstablishedAt = establishedAt;
        RecordEvent(new PackCreatedEvent(packId, packName));
    }

    /// <summary>
    /// Creates a new Pack with no Totem bound and no members.
    /// </summary>
    public static PackTotemAggregate Create(
        string packId,
        string packName,
        DateTimeOffset establishedAt)
    {
        return new PackTotemAggregate(packId, packName, establishedAt);
    }

    /// <summary>
    /// Constructs an aggregate from a previously persisted state. Does
    /// not record any domain event. Used by the Application persistence
    /// layer to rehydrate the aggregate from a <c>Document</c>.
    /// </summary>
    public static PackTotemAggregate Rehydrate(PackTotemState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var aggregate = new PackTotemAggregate();
        aggregate.PackId = state.PackId;
        aggregate.PackName = state.PackName;
        aggregate.members.AddRange(state.Members);
        aggregate.LeaderId = state.LeaderId;
        aggregate.TotemId = state.TotemId;
        aggregate.TotemRating = state.TotemRating;
        aggregate.totemImprovementPurchases.AddRange(state.TotemImprovementPurchases);
        aggregate.LinkState = state.LinkState;
        aggregate.activeTactics.AddRange(state.ActiveTactics);
        aggregate.LastTotemXpResolution = state.LastTotemXpResolution;
        aggregate.EstablishedAt = state.EstablishedAt;
        aggregate.DissolvedAt = state.DissolvedAt;
        return aggregate;
    }

    /// <summary>
    /// Captures the aggregate's current state for persistence. Does not
    /// mutate or record events.
    /// </summary>
    public PackTotemState CaptureState() => new(
        PackId,
        PackName,
        members.ToArray(),
        LeaderId,
        TotemId,
        TotemRating,
        totemImprovementPurchases.ToArray(),
        LinkState,
        activeTactics.ToArray(),
        LastTotemXpResolution,
        EstablishedAt,
        DissolvedAt);

    /// <summary>
    /// Adds a member to the Pack. Throws if the Pack is dissolved, the
    /// character identifier is empty, or the character is already a
    /// member (duplicate rejection).
    /// </summary>
    public void AddMember(string characterId)
    {
        EnsureNotDissolved();
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character identifier must not be empty.", nameof(characterId));
        }
        if (members.Contains(characterId, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Character '{characterId}' is already a member of Pack '{PackId}'.");
        }
        members.Add(characterId);
        RecordEvent(new PackMemberJoinedEvent(PackId, characterId));
    }

    /// <summary>
    /// Removes a member from the Pack. Throws if the Pack is dissolved
    /// or the character is not a member. Removing the current leader
    /// also clears the leader slot; a new leader must be set
    /// out-of-band (Pack succession is intentionally not implemented
    /// per the audit's SOURCE_UNSPECIFIED classification).
    /// </summary>
    public void RemoveMember(string characterId)
    {
        EnsureNotDissolved();
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character identifier must not be empty.", nameof(characterId));
        }
        if (!members.Remove(characterId))
        {
            throw new InvalidOperationException(
                $"Character '{characterId}' is not a member of Pack '{PackId}'.");
        }
        if (string.Equals(LeaderId, characterId, StringComparison.Ordinal))
        {
            LeaderId = null;
        }
        RecordEvent(new PackMemberLeftEvent(PackId, characterId));
    }

    /// <summary>
    /// Sets the Pack's leader. The leader must be a current member.
    /// Throws if the Pack is dissolved, the identifier is empty, or
    /// the character is not a member.
    /// </summary>
    public void SetLeader(string characterId)
    {
        EnsureNotDissolved();
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character identifier must not be empty.", nameof(characterId));
        }
        if (!members.Contains(characterId, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Cannot set leader: character '{characterId}' is not a member of Pack '{PackId}'.");
        }
        LeaderId = characterId;
    }

    /// <summary>
    /// Binds the Pack to a Totem. Throws if the Pack is dissolved, the
    /// Pack is already bound, the Totem identifier is empty, or the
    /// rating is not positive. <paramref name="totemAggregation"/> is
    /// the sum of member investments per source Line 1632. The
    /// A-012 XP-cost contradiction is preserved as
    /// <see cref="TotemXpResolutionState.Unresolved"/>.
    /// </summary>
    public void BindTotem(
        string totemId,
        int totemRating,
        int totemAggregation,
        IReadOnlyList<string> initialImprovementPurchases)
    {
        EnsureNotDissolved();
        if (LinkState != PackTotemLinkState.Unbound)
        {
            throw new InvalidOperationException(
                $"Pack '{PackId}' is already bound to Totem '{TotemId}' (state: {LinkState}).");
        }
        if (string.IsNullOrWhiteSpace(totemId))
        {
            throw new ArgumentException("Totem identifier must not be empty.", nameof(totemId));
        }
        if (totemRating <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totemRating), totemRating,
                "Totem rating must be positive.");
        }
        if (totemAggregation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totemAggregation), totemAggregation,
                "Totem aggregation must not be negative.");
        }

        TotemId = totemId;
        TotemRating = totemRating;
        LinkState = PackTotemLinkState.Bound;
        if (initialImprovementPurchases is not null)
        {
            foreach (var improvement in initialImprovementPurchases)
            {
                if (!string.IsNullOrWhiteSpace(improvement))
                {
                    totemImprovementPurchases.Add(improvement);
                }
            }
        }

        RecordEvent(new TotemBoundEvent(PackId, totemId, totemRating, totemAggregation));
    }

    /// <summary>
    /// Dissolves the Pack. After dissolution, the aggregate rejects all
    /// further mutations. Per source Line 199, dissolution releases the
    /// Totem; E2 records the dissolution but does not automatically
    /// mutate the <see cref="TotemId"/> (the Werewolf rule set owns the
    /// ceremonial narrative, and the aggregate is the persistent
    /// record-of-fact only).
    /// </summary>
    public void Dissolve(DateTimeOffset dissolvedAt)
    {
        EnsureNotDissolved();
        LinkState = PackTotemLinkState.Dissolving;
        DissolvedAt = dissolvedAt;
        RecordEvent(new PackDissolvedEvent(PackId));
    }

    private void EnsureNotDissolved()
    {
        if (IsDissolved)
        {
            throw new InvalidOperationException(
                $"Pack '{PackId}' is dissolved; mutations are not permitted.");
        }
    }
}
