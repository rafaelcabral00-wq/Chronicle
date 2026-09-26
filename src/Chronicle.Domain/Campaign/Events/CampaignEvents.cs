namespace Chronicle.Domain.Campaign.Events;

/// <summary>
/// Emitted when a new Campaign is created. Carries the persistent identity
/// and the name of the new Campaign.
/// </summary>
public sealed record CampaignCreatedEvent(
    string CampaignId,
    string CampaignName) : IDomainEvent;

/// <summary>
/// Emitted when a Campaign is closed.
/// </summary>
public sealed record CampaignClosedEvent(
    string CampaignId) : IDomainEvent;