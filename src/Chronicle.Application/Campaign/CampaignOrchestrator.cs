using Chronicle.Application.Persistence;
using Chronicle.Domain.Campaign;

namespace Chronicle.Application.Campaign;

/// <summary>
/// Result of a Campaign operation.
/// </summary>
public sealed record CampaignOperationResult(
    bool Succeeded,
    string? CampaignId,
    CampaignLifecycle? Lifecycle,
    string? FailureReason);

/// <summary>
/// Read-only result for retrieving a Campaign by its business identifier.
/// </summary>
public sealed record CampaignResult(
    bool Found,
    string? CampaignId,
    string? CampaignName,
    CampaignLifecycle? Lifecycle,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? ClosedAt,
    string? FailureReason);

/// <summary>
/// Application orchestrator for Campaign operations. Coordinates the
/// canonical <c>load → mutate → save</c> flow over the E1
/// <see cref="AggregateStore"/>, leaving domain invariants and event
/// recording to the aggregate itself.
/// </summary>
public sealed class CampaignOrchestrator
{
    private readonly AggregateStore aggregateStore;

    public CampaignOrchestrator(AggregateStore aggregateStore)
    {
        ArgumentNullException.ThrowIfNull(aggregateStore);
        this.aggregateStore = aggregateStore;
    }

    /// <summary>
    /// Creates a new Campaign and persists its initial state.
    /// </summary>
    public async Task<CampaignOperationResult> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        CampaignAggregate aggregate;
        try
        {
            aggregate = CampaignAggregate.Create(
                request.CampaignId,
                request.CampaignName,
                request.CreatedAt);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return new CampaignOperationResult(false, request.CampaignId, null, ex.Message);
        }

        var document = new Document(
            aggregate.Id,
            CampaignSerializer.ContentType,
            CampaignSerializer.Serialize(aggregate.CaptureState()),
            Version: 0);

        var save = await aggregateStore
            .SaveAsync(document, expectedVersion: null, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new CampaignOperationResult(true, aggregate.CampaignId, aggregate.Lifecycle, null),
            _ =>
                new CampaignOperationResult(
                    false, aggregate.CampaignId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }

    /// <summary>
    /// Resolves a Campaign's business identifier to the aggregate's
    /// persistence identifier. Returns <c>null</c> when no aggregate
    /// exists for the given <paramref name="campaignId"/>. The resolver
    /// performs a single repository scan through the existing
    /// <see cref="AggregateStore.EnumerateAsync"/>; it does not maintain
    /// a dedicated index and does not change the SQLite schema.
    /// </summary>
    public async Task<Guid?> FindByCampaignIdAsync(
        string campaignId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            throw new ArgumentException("Campaign identifier must not be empty.", nameof(campaignId));
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, CampaignSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = CampaignSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.CampaignId, campaignId, StringComparison.Ordinal))
            {
                return document.Id;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a Campaign by its business identifier and returns a read-only
    /// result without mutating any aggregate or persisting state.
    /// </summary>
    public async Task<CampaignResult> GetCampaignAsync(
        string campaignId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            return new CampaignResult(
                false, null, null, null, null, null, "Campaign identifier must not be empty.");
        }

        var documents = await aggregateStore
            .EnumerateAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            if (!string.Equals(document.ContentType, CampaignSerializer.ContentType, StringComparison.Ordinal))
            {
                continue;
            }

            var state = CampaignSerializer.Deserialize(document.PayloadJson);

            if (string.Equals(state.CampaignId, campaignId, StringComparison.Ordinal))
            {
                return new CampaignResult(
                    true,
                    state.CampaignId,
                    state.CampaignName,
                    state.Lifecycle,
                    state.CreatedAt,
                    state.ClosedAt,
                    null);
            }
        }

        return new CampaignResult(
            false, null, null, null, null, null, null);
    }

    /// <summary>
    /// Closes an existing Campaign. Loads the persistent aggregate,
    /// mutates it through the domain operation, and persists the
    /// resulting state.
    /// </summary>
    public async Task<CampaignOperationResult> CloseCampaignAsync(
        CloseCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var aggregateId = await FindByCampaignIdAsync(request.CampaignId, cancellationToken)
            .ConfigureAwait(false);
        if (aggregateId is null)
        {
            return new CampaignOperationResult(
                false, request.CampaignId, null, $"Campaign '{request.CampaignId}' was not found.");
        }

        return await MutateAggregateAsync(
            aggregateId.Value,
            request.CampaignId,
            aggregate => aggregate.Close(request.ClosedAt),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<CampaignOperationResult> MutateAggregateAsync(
        Guid aggregateId,
        string campaignId,
        Action<CampaignAggregate> mutation,
        CancellationToken cancellationToken)
    {
        var load = await aggregateStore
            .LoadAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false);

        if (load.Status == DocumentPersistenceStatus.NotFound)
        {
            return new CampaignOperationResult(
                false, campaignId, null, $"Campaign '{campaignId}' was not found.");
        }
        if (load.Status != DocumentPersistenceStatus.Succeeded || load.Document is null)
        {
            return new CampaignOperationResult(
                false, campaignId, null, load.FailureReason ?? load.Status.ToString());
        }

        CampaignAggregate aggregate;
        try
        {
            var state = CampaignSerializer.Deserialize(load.Document.PayloadJson);
            aggregate = CampaignAggregate.Rehydrate(state);
        }
        catch (Exception ex)
        {
            return new CampaignOperationResult(
                false, campaignId, null, $"Failed to rehydrate Campaign state: {ex.Message}");
        }

        try
        {
            mutation(aggregate);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
        {
            return new CampaignOperationResult(false, aggregate.CampaignId, aggregate.Lifecycle, ex.Message);
        }

        var updatedDocument = new Document(
            load.Document.Id,
            load.Document.ContentType,
            CampaignSerializer.Serialize(aggregate.CaptureState()),
            load.Document.Version);

        var save = await aggregateStore
            .SaveAsync(updatedDocument, expectedVersion: load.Document.Version, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new CampaignOperationResult(true, aggregate.CampaignId, aggregate.Lifecycle, null),
            _ =>
                new CampaignOperationResult(
                    false, aggregate.CampaignId, aggregate.Lifecycle, save.FailureReason ?? save.Status.ToString())
        };
    }
}

public sealed record CreateCampaignRequest(
    string CampaignId,
    string CampaignName,
    DateTimeOffset CreatedAt);

public sealed record CloseCampaignRequest(
    string CampaignId,
    DateTimeOffset ClosedAt);