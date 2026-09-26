using System.Text.Json;
using Chronicle.Application.Scene;
using Chronicle.Domain.Scene;
using Xunit;

namespace Chronicle.Application.Tests.Scene;

public sealed class SceneSerializerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SerializeProducesCamelCaseJson()
    {
        var state = new SceneState(
            SceneId: "scene-1",
            SessionId: "session-1",
            SceneName: "Forest Ambush",
            Lifecycle: SceneLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);

        var json = SceneSerializer.Serialize(state);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("scene-1", doc.RootElement.GetProperty("sceneId").GetString());
        Assert.Equal("session-1", doc.RootElement.GetProperty("sessionId").GetString());
        Assert.Equal("Forest Ambush", doc.RootElement.GetProperty("sceneName").GetString());
        Assert.Equal("active", doc.RootElement.GetProperty("lifecycle").GetString());
    }

    [Fact]
    public void SerializeClosedSceneProducesClosedLifecycle()
    {
        var state = new SceneState(
            SceneId: "scene-1",
            SessionId: "session-1",
            SceneName: "Forest Ambush",
            Lifecycle: SceneLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = SceneSerializer.Serialize(state);
        var deserialized = SceneSerializer.Deserialize(json);

        Assert.Equal(SceneLifecycle.Closed, deserialized.Lifecycle);
        Assert.Equal(Later, deserialized.ClosedAt);
    }

    [Fact]
    public void DeserializeRoundTripsAllFields()
    {
        var state = new SceneState(
            SceneId: "scene-1",
            SessionId: "session-1",
            SceneName: "Forest Ambush",
            Lifecycle: SceneLifecycle.Closed,
            CreatedAt: Now,
            ClosedAt: Later);

        var json = SceneSerializer.Serialize(state);
        var deserialized = SceneSerializer.Deserialize(json);

        Assert.Equal(state.SceneId, deserialized.SceneId);
        Assert.Equal(state.SessionId, deserialized.SessionId);
        Assert.Equal(state.SceneName, deserialized.SceneName);
        Assert.Equal(state.Lifecycle, deserialized.Lifecycle);
        Assert.Equal(state.CreatedAt, deserialized.CreatedAt);
        Assert.Equal(state.ClosedAt, deserialized.ClosedAt);
    }

    [Fact]
    public void SerializeRejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() => SceneSerializer.Serialize(null!));
    }

    [Fact]
    public void DeserializeRejectsEmptyPayload()
    {
        Assert.Throws<ArgumentException>(() => SceneSerializer.Deserialize(string.Empty));
    }
}