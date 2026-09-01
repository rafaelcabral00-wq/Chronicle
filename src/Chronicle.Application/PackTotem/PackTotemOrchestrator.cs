using Chronicle.Application.Persistence;
using Chronicle.Domain.PackTotem;

namespace Chronicle.Application.PackTotem;

/// <summary>
/// Result of a Pack/Totem operation. Provider-neutral; mirrors the
/// existing <c>ResourceTransitionResult</c> shape used elsewhere in
/// the Application layer.
/// </summary>
public sealed record PackTotemOperationResult(
    bool Succeeded,
    string? PackId,
    PackTotemLinkState? LinkState,
    string? FailureReason);

/// <summary>
/// Application orchestrator for Pack/Totem operations. Coordinates the
/// canonical <c>load → mutate → save</c> flow over the E1
/// <see cref="AggregateStore"/>, leaving domain invariants and event
/// recording to the aggregate itself.
/// </summary>
public sealed class PackTotemOrchestrator
{
    private readonly AggregateStore aggregateStore;

    public PackTotemOrchestrator(AggregateStore aggregateStore)
    {
        ArgumentNullException.ThrowIfNull(aggregateStore);
        this.aggregateStore = aggregateStore;
    }

    /// <summary>
    /// Creates a new Pack and persists its initial state.
    /// </summary>
    public async Task<PackTotemOperationResult> CreatePackAsync(
        CreatePackRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        PackTotemAggregate aggregate;
        try
        {
            aggregate = PackTotemAggregate.Create(
                request.PackId,
                request.PackName,
                request.EstablishedAt);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return new PackTotemOperationResult(false, request.PackId, null, ex.Message);
        }

        var document = new Document(
            aggregate.Id,
            PackTotemSerializer.ContentType,
            PackTotemSerializer.Serialize(aggregate.CaptureState()),
            Version: 0);

        var save = await aggregateStore
            .SaveAsync(document, expectedVersion: null, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new PackTotemOperationResult(true, aggregate.PackId, aggregate.LinkState, null),
            _ =>
                new PackTotemOperationResult(
                    false, aggregate.PackId, aggregate.LinkState, save.FailureReason ?? save.Status.ToString())
        };
    }

    /// <summary>
    /// Binds a Totem to an existing Pack. Loads the persistent
    /// aggregate, mutates it through the domain operation, and
    /// persists the resulting state. The orchestrator does not
    /// publish domain events; the events remain queued on the
    /// aggregate and are available to a future dispatch mechanism.
    /// </summary>
    public async Task<PackTotemOperationResult> BindTotemAsync(
        BindTotemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var load = await aggregateStore
            .LoadAsync(request.PackIdAggregateId, cancellationToken)
            .ConfigureAwait(false);

        if (load.Status == DocumentPersistenceStatus.NotFound)
        {
            return new PackTotemOperationResult(
                false, request.PackId, null, $"Pack '{request.PackId}' was not found.");
        }
        if (load.Status != DocumentPersistenceStatus.Succeeded || load.Document is null)
        {
            return new PackTotemOperationResult(
                false, request.PackId, null, load.FailureReason ?? load.Status.ToString());
        }

        PackTotemAggregate aggregate;
        try
        {
            var state = PackTotemSerializer.Deserialize(load.Document.PayloadJson);
            aggregate = PackTotemAggregate.Rehydrate(state);
        }
        catch (Exception ex)
        {
            return new PackTotemOperationResult(
                false, request.PackId, null, $"Failed to rehydrate Pack state: {ex.Message}");
        }

        try
        {
            aggregate.BindTotem(
                request.TotemId,
                request.TotemRating,
                request.TotemAggregation,
                request.InitialImprovementPurchases);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
        {
            return new PackTotemOperationResult(false, aggregate.PackId, aggregate.LinkState, ex.Message);
        }

        var updatedDocument = new Document(
            load.Document.Id,
            load.Document.ContentType,
            PackTotemSerializer.Serialize(aggregate.CaptureState()),
            load.Document.Version);

        var save = await aggregateStore
            .SaveAsync(updatedDocument, expectedVersion: load.Document.Version, cancellationToken)
            .ConfigureAwait(false);

        return save.Status switch
        {
            DocumentPersistenceStatus.Succeeded =>
                new PackTotemOperationResult(true, aggregate.PackId, aggregate.LinkState, null),
            _ =>
                new PackTotemOperationResult(
                    false, aggregate.PackId, aggregate.LinkState, save.FailureReason ?? save.Status.ToString())
        };
    }
}

public sealed record CreatePackRequest(
    string PackId,
    string PackName,
    DateTimeOffset EstablishedAt);

/// <summary>
/// Application-side request for binding a Totem to a Pack. Mirrors the
/// fields of the existing S4 Werewolf
/// <c>WerewolfTotemBindingBoundaryPayload</c>; the composition root is
/// responsible for translating the typed boundary into this
/// provider-neutral request so that Application does not depend on the
/// Werewolf rule set assembly.
/// </summary>
public sealed record BindTotemRequest(
    Guid PackIdAggregateId,
    string PackId,
    string TotemId,
    int TotemRating,
    int TotemAggregation,
    IReadOnlyList<string> InitialImprovementPurchases);
