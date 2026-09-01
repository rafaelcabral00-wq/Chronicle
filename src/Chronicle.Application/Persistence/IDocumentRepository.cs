namespace Chronicle.Application.Persistence;

/// <summary>
/// Provider-neutral persistence contract for a generic document.
/// <para>
/// The Application layer uses this abstraction to load and persist
/// aggregate state without knowing the concrete storage engine.
/// Concrete implementations live in <c>Chronicle.Persistence.*</c>.
/// </para>
/// <para>
/// E1 deliberately exposes only load, save, and delete. Bulk operations,
/// querying, search, restore, archive, and snapshot APIs are intentionally
/// absent and will only be introduced when a concrete requirement
/// demonstrates a need.
/// </para>
/// <para>
/// Optimistic concurrency uses a monotonically increasing <c>Version</c>
/// field on the <see cref="Document"/>. A <c>null</c>
/// <c>expectedVersion</c> on <see cref="SaveAsync"/> means "create-only";
/// a value means "update-only and must match the stored version".
/// </para>
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Loads the document with the given identity.
    /// Returns <see cref="DocumentPersistenceStatus.NotFound"/> when absent.
    /// </summary>
    Task<DocumentPersistenceResult> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the document. On create, <paramref name="expectedVersion"/> must
    /// be null and the document's <c>Version</c> must be 0. On update,
    /// <paramref name="expectedVersion"/> must equal the stored version.
    /// On success, the returned document has its version incremented.
    /// </summary>
    Task<DocumentPersistenceResult> SaveAsync(
        Document document,
        long? expectedVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the document with the given identity. Returns
    /// <see cref="DocumentPersistenceStatus.NotFound"/> when absent.
    /// </summary>
    Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
