namespace Chronicle.Application.Persistence;

/// <summary>
/// Result of a persistence operation. Provider-neutral; concrete
/// implementations (e.g. SQLite) return this same record.
/// </summary>
public enum DocumentPersistenceStatus
{
    /// <summary>Operation completed successfully.</summary>
    Succeeded,
    /// <summary>The document was not found.</summary>
    NotFound,
    /// <summary>The supplied <c>ExpectedVersion</c> did not match the stored version.</summary>
    ConcurrencyConflict,
    /// <summary>The persistence layer failed; see <c>FailureReason</c>.</summary>
    PersistenceFailure
}

public sealed record DocumentPersistenceResult(
    DocumentPersistenceStatus Status,
    Document? Document,
    string? FailureReason);
