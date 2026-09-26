using Chronicle.Application.Persistence;
using Chronicle.Domain.Session;

namespace Chronicle.Application.Session;

/// <summary>
/// Result of a Session operation.
/// </summary>
public sealed record SessionOperationResult(
    bool Succeeded,
    string? SessionId,
    SessionLifecycle? Lifecycle,
    string? FailureReason);

/// <summary>
/// Read-only result for retrieving a Session by its business identifier.
/// </summary>
public sealed record SessionResult(
    bool Found,
    string? SessionId,
    string? CampaignId,
    string? SessionName,
    SessionLifecycle? Lifecycle,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? ClosedAt,
    string? FailureReason);

/// <summary>
/// Application orchestrator for Session operations. Coordinates the
/// canonical <c>load → mutate → save</c> flow over the E1
/// <see cref="AggregateStore"/>, leaving domain invariants and event
/// recording to the aggregate itself.
/// </summary>
public sealed class SessionOrchestrator
{
    private readonly AggregateStore aggregateStore;

    public SessionOrchestrator(AggregateStore aggregateStore)
    {
        ArgumentNullException.ThrowIfNull(aggregateStore);
        this.aggregateStore = aggregateStore;
    }

    /// <summary>
    /// Creates a new Session and persists its initial state.
    /// </summary>
    public async Task<SessionOperationResult> CreateSessionAsync(
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SessionAggregate aggregate;
        try
        {
            aggregate = SessionAggregate.Create(
                request.SessionId,
                request.CampaignId,
                request.SessionName,
                request.CreatedAt);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return new SessionOperationResult(false, request.SessionId, null, ex.Message);
        }

        var document = new Document(
            aggregate.Id,
            SessionSerializer.ContentType,
            SessionSerializer.Serialize(aggregate.CaptureState()),
            Version: 0);

        var save = await aggregateStore
            .SaveAsync(document, expectedVersion: null, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new SessionOperationResult(true, aggregate.SessionId, aggregate.Lifecycle, null),
            _ =>
                new SessionOperationResult(
                    false, aggregate.SessionId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }

    /// <summary>
    /// Resolves a Session's business identifier to the aggregate's
    /// persistence identifier. Returns <c>null</c> when no aggregate
    /// exists for the given <paramref name="sessionId"/>. The resolver
    /// performs a single repository scan through the existing
    /// <see cref="AggregateStore.EnumerateAsync"/>; it does not maintain
    /// a dedicated index and does not change the SQLite schema.
    /// </summary>
    public async Task<Guid?> FindBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session identifier must not be empty.", nameof(sessionId));
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, SessionSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = SessionSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.SessionId, sessionId, StringComparison.Ordinal))
            {
                return document.Id;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a Session by its business identifier and returns a read-only
    /// result without mutating any aggregate or persisting state.
    /// </summary>
    public async Task<SessionResult> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return new SessionResult(
                false, null, null, null, null, null, null, "Session identifier must not be empty.");
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, SessionSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = SessionSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.SessionId, sessionId, StringComparison.Ordinal))
            {
                return new SessionResult(
                    true,
                    state.SessionId,
                    state.CampaignId,
                    state.SessionName,
                    state.Lifecycle,
                    state.CreatedAt,
                    state.ClosedAt,
                    null);
            }
        }

        return new SessionResult(
            false, null, null, null, null, null, null, null);
    }

    /// <summary>
    /// Closes an existing Session. Loads the persistent aggregate,
    /// mutates it through the domain operation, and persists the
    /// resulting state.
    /// </summary>
    public async Task<SessionOperationResult> CloseSessionAsync(
        CloseSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var aggregateId = await FindBySessionIdAsync(request.SessionId, cancellationToken)
            .ConfigureAwait(false);
        if (aggregateId is null)
        {
            return new SessionOperationResult(
                false, request.SessionId, null, $"Session '{request.SessionId}' was not found.");
        }

        return await MutateAggregateAsync(
            aggregateId.Value,
            request.SessionId,
            aggregate => aggregate.Close(request.ClosedAt),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<SessionOperationResult> MutateAggregateAsync(
        Guid aggregateId,
        string sessionId,
        Action<SessionAggregate> mutation,
        CancellationToken cancellationToken)
    {
        var load = await aggregateStore
            .LoadAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false);

        if (load.Status == DocumentPersistenceStatus.NotFound)
        {
            return new SessionOperationResult(
                false, sessionId, null, $"Session '{sessionId}' was not found.");
        }
        if (load.Status != DocumentPersistenceStatus.Succeeded || load.Document is null)
        {
            return new SessionOperationResult(
                false, sessionId, null, load.FailureReason ?? load.Status.ToString());
        }

        SessionAggregate aggregate;
        try
        {
            var state = SessionSerializer.Deserialize(load.Document.PayloadJson);
            aggregate = SessionAggregate.Rehydrate(state);
        }
        catch (Exception ex)
        {
            return new SessionOperationResult(
                false, sessionId, null, $"Failed to rehydrate Session state: {ex.Message}");
        }

        try
        {
            mutation(aggregate);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
        {
            return new SessionOperationResult(false, aggregate.SessionId, aggregate.Lifecycle, ex.Message);
        }

        var updatedDocument = new Document(
            load.Document.Id,
            load.Document.ContentType,
            SessionSerializer.Serialize(aggregate.CaptureState()),
            load.Document.Version);

        var save = await aggregateStore
            .SaveAsync(updatedDocument, expectedVersion: load.Document.Version, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new SessionOperationResult(true, aggregate.SessionId, aggregate.Lifecycle, null),
            _ =>
                new SessionOperationResult(
                    false, aggregate.SessionId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }
}

public sealed record CreateSessionRequest(
    string SessionId,
    string CampaignId,
    string SessionName,
    DateTimeOffset CreatedAt);

public sealed record CloseSessionRequest(
    string SessionId,
    DateTimeOffset ClosedAt);