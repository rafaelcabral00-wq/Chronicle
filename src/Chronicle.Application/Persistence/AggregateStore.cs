namespace Chronicle.Application.Persistence;

/// <summary>
/// Generic Application-side orchestrator for aggregate persistence.
/// <para>
/// Coordinates the canonical <c>load → mutate → save</c> flow without
/// knowing any concrete aggregate type. The orchestrator is intentionally
/// aggregate-agnostic; the caller serialises the aggregate state to JSON
/// before <see cref="SaveAsync"/> and rehydrates it from JSON after
/// <see cref="LoadAsync"/>.
/// </para>
/// <para>
/// E1 deliberately does not implement a command bus, handler registry, or
/// mediator. The single-responsibility orchestrator pattern matches the
/// existing <c>ResourceTransitionOrchestrator</c> convention already
/// established in <c>Chronicle.Application</c>.
/// </para>
/// </summary>
public sealed class AggregateStore
{
    private readonly IDocumentRepository repository;

    public AggregateStore(IDocumentRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        this.repository = repository;
    }

    /// <summary>
    /// Loads the persisted state for the given aggregate identity.
    /// </summary>
    public async Task<AggregateLoadResult> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.LoadAsync(aggregateId, cancellationToken).ConfigureAwait(false);
        return new AggregateLoadResult(result.Status, result.Document, result.FailureReason);
    }

    /// <summary>
    /// Persists the document. Returns the post-save document with the
    /// incremented version, or a status describing why the save failed.
    /// </summary>
    public async Task<AggregateSaveResult> SaveAsync(
        Document document,
        long? expectedVersion,
        CancellationToken cancellationToken = default)
    {
        var result = await repository
            .SaveAsync(document, expectedVersion, cancellationToken)
            .ConfigureAwait(false);
        return new AggregateSaveResult(result.Status, result.Document, result.FailureReason);
    }

    /// <summary>
    /// Removes the document with the given identity.
    /// </summary>
    public async Task<AggregateDeleteResult> DeleteAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var result = await repository
            .DeleteAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false);
        return new AggregateDeleteResult(result.Status, result.FailureReason);
    }
}

public sealed record AggregateLoadResult(
    DocumentPersistenceStatus Status,
    Document? Document,
    string? FailureReason);

public sealed record AggregateSaveResult(
    DocumentPersistenceStatus Status,
    Document? Document,
    string? FailureReason);

public sealed record AggregateDeleteResult(
    DocumentPersistenceStatus Status,
    string? FailureReason);
