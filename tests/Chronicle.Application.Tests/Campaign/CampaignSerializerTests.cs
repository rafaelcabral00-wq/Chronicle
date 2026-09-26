using System.Text.Json;
using Chronicle.Application.Campaign;
using Chronicle.Domain.Campaign;
using Xunit;

namespace Chronicle.Application.Tests.Campaign;

public sealed class CampaignSerializerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SerializeProducesCamelCaseJson()
    {
        var state = new CampaignState(
            CampaignId: "campaign-1",
            CampaignName: "The Iron Chronicles",
            Lifecycle: CampaignLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);

        var json = CampaignSerializer.Serialize(state);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("campaign-1", doc.RootElement.GetProperty("campaignId").GetString());
        Assert.Equal("active", doc.RootElement.GetProperty("lifecycle").GetString());
    }

    [Fact]
    public void SerializeClosedCampaignProducesClosedLifecycle()
    {
        var state = new CampaignState(
            CampaignId: "campaign-1",
            CampaignName: "The Iron Chronicles",
            Lifecycle: CampaignLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = CampaignSerializer.Serialize(state);
        var deserialized = CampaignSerializer.Deserialize(json);

        Assert.Equal(CampaignLifecycle.Closed, deserialized.Lifecycle);
        Assert.Equal(Later, deserialized.ClosedAt);
    }

    [Fact]
    public void DeserializeRoundTripsAllFields()
    {
        var state = new CampaignState(
            CampaignId: "campaign-1",
            CampaignName: "The Iron Chronicles",
            Lifecycle: CampaignLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = CampaignSerializer.Serialize(state);
        var deserialized = CampaignSerializer.Deserialize(json);

        Assert.Equal(state.CampaignId, deserialized.CampaignId);
        Assert.Equal(state.CampaignName, deserialized.CampaignName);
        Assert.Equal(state.Lifecycle, deserialized.Lifecycle);
        Assert.Equal(state.CreatedAt, deserialized.CreatedAt);
        Assert.Equal(state.ClosedAt, deserialized.ClosedAt);
    }

    [Fact]
    public void SerializeRejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() => CampaignSerializer.Serialize(null!));
    }

    [Fact]
    public void DeserializeRejectsEmptyPayload()
    {
        Assert.Throws<ArgumentException>(() => CampaignSerializer.Deserialize(string.Empty));
    }
}