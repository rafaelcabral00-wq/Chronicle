namespace Chronicle.Domain.Session;

/// <summary>
/// Represents the lifecycle state of a Session.
/// </summary>
public enum SessionLifecycle
{
    /// <summary>
    /// The session is active and can be mutated.
    /// </summary>
    Active,

    /// <summary>
    /// The session is closed and rejects all mutations.
    /// </summary>
    Closed
}