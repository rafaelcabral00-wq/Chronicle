using System.Text.Json;
using Chronicle.Application.Session;
using Chronicle.Domain.Session;
using Xunit;

namespace Chronicle.Application.Tests.Session;

public sealed class SessionSerializerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SerializeProducesCamelCaseJson()
    {
        var state = new SessionState(
            SessionId: "session-1",
            CampaignId: "campaign-1",
            SessionName: "First Session",
            Lifecycle: SessionLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);

        var json = SessionSerializer.Serialize(state);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("session-1", doc.RootElement.GetProperty("sessionId").GetString());
        Assert.Equal("campaign-1", doc.RootElement.GetProperty("campaignId").GetString());
        Assert.Equal("active", doc.RootElement.GetProperty("lifecycle").GetString());
    }

    [Fact]
    public void SerializeClosedSessionProducesClosedLifecycle()
    {
        var state = new SessionState(
            SessionId: "session-1",
            CampaignId: "campaign-1",
            SessionName: "First Session",
            Lifecycle: SessionLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = SessionSerializer.Serialize(state);
        var deserialized = SessionSerializer.Deserialize(json);

        Assert.Equal(SessionLifecycle.Closed, deserialized.Lifecycle);
        Assert.Equal(Later, deserialized.ClosedAt);
    }

    [Fact]
    public void DeserializeRoundTripsAllFields()
    {
        var state = new SessionState(
            SessionId: "session-1",
            CampaignId: "campaign-1",
            SessionName: "First Session",
            Lifecycle: SessionLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = SessionSerializer.Serialize(state);
        var deserialized = SessionSerializer.Deserialize(json);

        Assert.Equal(state.SessionId, deserialized.SessionId);
        Assert.Equal(state.CampaignId, deserialized.CampaignId);
        Assert.Equal(state.SessionName, deserialized.SessionName);
        Assert.Equal(state.Lifecycle, deserialized.Lifecycle);
        Assert.Equal(state.CreatedAt, deserialized.CreatedAt);
        Assert.Equal(state.ClosedAt, deserialized.ClosedAt);
    }

    [Fact]
    public void SerializeRejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() => SessionSerializer.Serialize(null!));
    }

    [Fact]
    public void DeserializeRejectsEmptyPayload()
    {
        Assert.Throws<ArgumentException>(() => SessionSerializer.Deserialize(string.Empty));
    }
}