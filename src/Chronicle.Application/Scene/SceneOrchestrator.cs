using Chronicle.Application.Persistence;
using Chronicle.Domain.Scene;

namespace Chronicle.Application.Scene;

/// <summary>
/// Result of a Scene operation.
/// </summary>
public sealed record SceneOperationResult(
    bool Succeeded,
    string? SceneId,
    SceneLifecycle? Lifecycle,
    string? FailureReason);

/// <summary>
/// Read-only result for retrieving a Scene by its business identifier.
/// </summary>
public sealed record SceneResult(
    bool Found,
    string? SceneId,
    string? SessionId,
    string? SceneName,
    SceneLifecycle? Lifecycle,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? ClosedAt,
    string? FailureReason);

/// <summary>
/// Application orchestrator for Scene operations. Coordinates the
/// canonical <c>load → mutate → save</c> flow over the E1
/// <see cref="AggregateStore"/>, leaving domain invariants and event
/// recording to the aggregate itself.
/// </summary>
public sealed class SceneOrchestrator
{
    private readonly AggregateStore aggregateStore;

    public SceneOrchestrator(AggregateStore aggregateStore)
    {
        ArgumentNullException.ThrowIfNull(aggregateStore);
        this.aggregateStore = aggregateStore;
    }

    /// <summary>
    /// Creates a new Scene and persists its initial state.
    /// </summary>
    public async Task<SceneOperationResult> CreateSceneAsync(
        CreateSceneRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SceneAggregate aggregate;
        try
        {
            aggregate = SceneAggregate.Create(
                request.SceneId,
                request.SessionId,
                request.SceneName,
                request.CreatedAt);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return new SceneOperationResult(false, request.SceneId, null, ex.Message);
        }

        var document = new Document(
            aggregate.Id,
            SceneSerializer.ContentType,
            SceneSerializer.Serialize(aggregate.CaptureState()),
            Version: 0);

        var save = await aggregateStore
            .SaveAsync(document, expectedVersion: null, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new SceneOperationResult(true, aggregate.SceneId, aggregate.Lifecycle, null),
            _ =>
                new SceneOperationResult(
                    false, aggregate.SceneId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }

    /// <summary>
    /// Resolves a Scene's business identifier to the aggregate's
    /// persistence identifier. Returns <c>null</c> when no aggregate
    /// exists for the given <paramref name="sceneId"/>. The resolver
    /// performs a single repository scan through the existing
    /// <see cref="AggregateStore.EnumerateAsync"/>; it does not maintain
    /// a dedicated index and does not change the SQLite schema.
    /// </summary>
    public async Task<Guid?> FindBySceneIdAsync(
        string sceneId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            throw new ArgumentException("Scene identifier must not be empty.", nameof(sceneId));
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, SceneSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = SceneSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.SceneId, sceneId, StringComparison.Ordinal))
            {
                return document.Id;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a Scene by its business identifier and returns a read-only
    /// result without mutating any aggregate or persisting state.
    /// </summary>
    public async Task<SceneResult> GetSceneAsync(
        string sceneId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            return new SceneResult(
                false, null, null, null, null, null, null, "Scene identifier must not be empty.");
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, SceneSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = SceneSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.SceneId, sceneId, StringComparison.Ordinal))
            {
                return new SceneResult(
                    true,
                    state.SceneId,
                    state.SessionId,
                    state.SceneName,
                    state.Lifecycle,
                    state.CreatedAt,
                    state.ClosedAt,
                    null);
            }
        }

        return new SceneResult(
            false, null, null, null, null, null, null, null);
    }

    /// <summary>
    /// Closes an existing Scene. Loads the persistent aggregate,
    /// mutates it through the domain operation, and persists the
    /// resulting state.
    /// </summary>
    public async Task<SceneOperationResult> CloseSceneAsync(
        CloseSceneRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var aggregateId = await FindBySceneIdAsync(request.SceneId, cancellationToken)
            .ConfigureAwait(false);
        if (aggregateId is null)
        {
            return new SceneOperationResult(
                false, request.SceneId, null, $"Scene '{request.SceneId}' was not found.");
        }

        return await MutateAggregateAsync(
            aggregateId.Value,
            request.SceneId,
            aggregate => aggregate.Close(request.ClosedAt),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<SceneOperationResult> MutateAggregateAsync(
        Guid aggregateId,
        string sceneId,
        Action<SceneAggregate> mutation,
        CancellationToken cancellationToken)
    {
        var load = await aggregateStore
            .LoadAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false);

        if (load.Status == DocumentPersistenceStatus.NotFound)
        {
            return new SceneOperationResult(
                false, sceneId, null, $"Scene '{sceneId}' was not found.");
        }
        if (load.Status != DocumentPersistenceStatus.Succeeded || load.Document is null)
        {
            return new SceneOperationResult(
                false, sceneId, null, load.FailureReason ?? load.Status.ToString());
        }

        SceneAggregate aggregate;
        try
        {
            var state = SceneSerializer.Deserialize(load.Document.PayloadJson);
            aggregate = SceneAggregate.Rehydrate(state);
        }
        catch (Exception ex)
        {
            return new SceneOperationResult(
                false, sceneId, null, $"Failed to rehydrate Scene state: {ex.Message}");
        }

        try
        {
            mutation(aggregate);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
        {
            return new SceneOperationResult(false, aggregate.SceneId, aggregate.Lifecycle, ex.Message);
        }

        var updatedDocument = new Document(
            load.Document.Id,
            load.Document.ContentType,
            SceneSerializer.Serialize(aggregate.CaptureState()),
            load.Document.Version);

        var save = await aggregateStore
            .SaveAsync(updatedDocument, expectedVersion: load.Document.Version, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new SceneOperationResult(true, aggregate.SceneId, aggregate.Lifecycle, null),
            _ =>
                new SceneOperationResult(
                    false, aggregate.SceneId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }
}

public sealed record CreateSceneRequest(
    string SceneId,
    string SessionId,
    string SceneName,
    DateTimeOffset CreatedAt);

public sealed record CloseSceneRequest(
    string SceneId,
    DateTimeOffset ClosedAt);