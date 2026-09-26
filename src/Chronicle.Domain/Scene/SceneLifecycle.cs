namespace Chronicle.Domain.Scene;

/// <summary>
/// Represents the lifecycle state of a Scene.
/// </summary>
public enum SceneLifecycle
{
    /// <summary>
    /// The scene is active and can be mutated.
    /// </summary>
    Active,

    /// <summary>
    /// The scene is closed and rejects all mutations.
    /// </summary>
    Closed
}