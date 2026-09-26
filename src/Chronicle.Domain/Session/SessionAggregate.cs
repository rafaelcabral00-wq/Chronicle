using Chronicle.Domain.Session.Events;

namespace Chronicle.Domain.Session;

/// <summary>
/// Chronicle Domain aggregate representing a Session.
/// <para>
/// A Session belongs to a Campaign but is an independent aggregate.
/// It stores only the minimum identifiers, lifecycle state, and timestamps.
/// Rule-set-specific mechanics (scenes, acts, participants, world state,
/// narrative turns, dice, LLM state) are intentionally not part of this aggregate.
/// </para>
/// <para>
/// The aggregate mutates only through domain methods. Each mutation
/// records exactly one <see cref="IDomainEvent"/> through the inherited
/// <c>RecordEvent</c> mechanism. Mutations after closure throw
/// <see cref="InvalidOperationException"/>; the aggregate does not
/// silently no-op.
/// </para>
/// </summary>
public sealed class SessionAggregate : AggregateRoot
{
    private SessionAggregate()
    {
    }

    private SessionAggregate(string sessionId, string campaignId, string sessionName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session identifier must not be empty.", nameof(sessionId));
        }
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            throw new ArgumentException("Campaign identifier must not be empty.", nameof(campaignId));
        }
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            throw new ArgumentException("Session name must not be empty.", nameof(sessionName));
        }

        SessionId = sessionId;
        CampaignId = campaignId;
        SessionName = sessionName;
        Lifecycle = SessionLifecycle.Active;
        CreatedAt = createdAt;
        RecordEvent(new SessionCreatedEvent(sessionId, campaignId, sessionName));
    }

    public string SessionId { get; private set; } = string.Empty;
    public string CampaignId { get; private set; } = string.Empty;
    public string SessionName { get; private set; } = string.Empty;
    public SessionLifecycle Lifecycle { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>
    /// Creates a new Session with Active lifecycle.
    /// </summary>
    public static SessionAggregate Create(
        string sessionId,
        string campaignId,
        string sessionName,
        DateTimeOffset createdAt)
    {
        return new SessionAggregate(sessionId, campaignId, sessionName, createdAt);
    }

    /// <summary>
    /// Constructs an aggregate from a previously persisted state. Does
    /// not record any domain event. Used by the Application persistence
    /// layer to rehydrate the aggregate from a <c>Document</c>.
    /// </summary>
    public static SessionAggregate Rehydrate(SessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var aggregate = new SessionAggregate();
        aggregate.SessionId = state.SessionId;
        aggregate.CampaignId = state.CampaignId;
        aggregate.SessionName = state.SessionName;
        aggregate.Lifecycle = state.Lifecycle;
        aggregate.CreatedAt = state.CreatedAt;
        aggregate.ClosedAt = state.ClosedAt;
        return aggregate;
    }

    /// <summary>
    /// Captures the aggregate's current state for persistence. Does not
    /// mutate or record events.
    /// </summary>
    public SessionState CaptureState() => new(
        SessionId,
        CampaignId,
        SessionName,
        Lifecycle,
        CreatedAt,
        ClosedAt);

    /// <summary>
    /// Closes the Session. After closure, the aggregate rejects all
    /// further mutations.
    /// </summary>
    public void Close(DateTimeOffset closedAt)
    {
        EnsureNotClosed();
        Lifecycle = SessionLifecycle.Closed;
        ClosedAt = closedAt;
        RecordEvent(new SessionClosedEvent(SessionId));
    }

    private void EnsureNotClosed()
    {
        if (Lifecycle == SessionLifecycle.Closed)
        {
            throw new InvalidOperationException(
                $"Session '{SessionId}' is closed; mutations are not permitted.");
        }
    }
}