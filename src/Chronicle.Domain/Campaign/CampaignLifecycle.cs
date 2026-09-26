namespace Chronicle.Domain.Campaign;

/// <summary>
/// Represents the lifecycle state of a Campaign.
/// </summary>
public enum CampaignLifecycle
{
    /// <summary>
    /// The campaign is active and can be mutated.
    /// </summary>
    Active,

    /// <summary>
    /// The campaign is closed and rejects all mutations.
    /// </summary>
    Closed
}