using Chronicle.Domain.Campaign.Events;

namespace Chronicle.Domain.Campaign;

/// <summary>
/// Chronicle Domain aggregate representing a Campaign.
/// <para>
/// A Campaign is a top-level narrative container independent of any
/// specific RPG rule set. It stores only the minimum identifiers,
/// lifecycle state, and timestamps. Rule-set-specific mechanics
/// (sessions, scenes, participants, world state) are intentionally
/// not part of this aggregate.
/// </para>
/// <para>
/// The aggregate mutates only through domain methods. Each mutation
/// records exactly one <see cref="IDomainEvent"/> through the inherited
/// <c>RecordEvent</c> mechanism. Mutations after closure throw
/// <see cref="InvalidOperationException"/>; the aggregate does not
/// silently no-op.
/// </para>
/// </summary>
public sealed class CampaignAggregate : AggregateRoot
{
    private CampaignAggregate()
    {
    }

    private CampaignAggregate(string campaignId, string campaignName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            throw new ArgumentException("Campaign identifier must not be empty.", nameof(campaignId));
        }
        if (string.IsNullOrWhiteSpace(campaignName))
        {
            throw new ArgumentException("Campaign name must not be empty.", nameof(campaignName));
        }

        CampaignId = campaignId;
        CampaignName = campaignName;
        Lifecycle = CampaignLifecycle.Active;
        CreatedAt = createdAt;
        RecordEvent(new CampaignCreatedEvent(campaignId, campaignName));
    }

    public string CampaignId { get; private set; } = string.Empty;
    public string CampaignName { get; private set; } = string.Empty;
    public CampaignLifecycle Lifecycle { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>
    /// Creates a new Campaign with Active lifecycle.
    /// </summary>
    public static CampaignAggregate Create(
        string campaignId,
        string campaignName,
        DateTimeOffset createdAt)
    {
        return new CampaignAggregate(campaignId, campaignName, createdAt);
    }

    /// <summary>
    /// Constructs an aggregate from a previously persisted state. Does
    /// not record any domain event. Used by the Application persistence
    /// layer to rehydrate the aggregate from a <c>Document</c>.
    /// </summary>
    public static CampaignAggregate Rehydrate(CampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var aggregate = new CampaignAggregate();
        aggregate.CampaignId = state.CampaignId;
        aggregate.CampaignName = state.CampaignName;
        aggregate.Lifecycle = state.Lifecycle;
        aggregate.CreatedAt = state.CreatedAt;
        aggregate.ClosedAt = state.ClosedAt;
        return aggregate;
    }

    /// <summary>
    /// Captures the aggregate's current state for persistence. Does not
    /// mutate or record events.
    /// </summary>
    public CampaignState CaptureState() => new(
        CampaignId,
        CampaignName,
        Lifecycle,
        CreatedAt,
        ClosedAt);

    /// <summary>
    /// Closes the Campaign. After closure, the aggregate rejects all
    /// further mutations.
    /// </summary>
    public void Close(DateTimeOffset closedAt)
    {
        EnsureNotClosed();
        Lifecycle = CampaignLifecycle.Closed;
        ClosedAt = closedAt;
        RecordEvent(new CampaignClosedEvent(CampaignId));
    }

    private void EnsureNotClosed()
    {
        if (Lifecycle == CampaignLifecycle.Closed)
        {
            throw new InvalidOperationException(
                $"Campaign '{CampaignId}' is closed; mutations are not permitted.");
        }
    }
}