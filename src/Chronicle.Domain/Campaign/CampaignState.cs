namespace Chronicle.Domain.Campaign;

/// <summary>
/// Provider-neutral snapshot of a <see cref="CampaignAggregate"/>'s
/// persistent state. Captured via
/// <see cref="CampaignAggregate.CaptureState"/> and rehydrated via
/// <see cref="CampaignAggregate.Rehydrate"/>. The Application layer
/// is responsible for serialising this record to a
/// <c>Document.PayloadJson</c>; Domain has no JSON attributes.
/// </summary>
public sealed record CampaignState(
    string CampaignId,
    string CampaignName,
    CampaignLifecycle Lifecycle,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);