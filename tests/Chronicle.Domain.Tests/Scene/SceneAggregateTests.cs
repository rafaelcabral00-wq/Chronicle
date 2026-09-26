using Chronicle.Domain.Scene;
using Chronicle.Domain.Scene.Events;
using Xunit;

namespace Chronicle.Domain.Tests.Scene;

public sealed class SceneAggregateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateInitializesSceneIdSessionIdAndName()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);

        Assert.Equal("scene-1", aggregate.SceneId);
        Assert.Equal("session-1", aggregate.SessionId);
        Assert.Equal("Forest Ambush", aggregate.SceneName);
        Assert.Equal(SceneLifecycle.Active, aggregate.Lifecycle);
        Assert.Equal(Now, aggregate.CreatedAt);
        Assert.Null(aggregate.ClosedAt);
    }

    [Fact]
    public void CreateEmitsSceneCreatedEvent()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);

        var evt = Assert.Single(aggregate.UncommittedEvents);
        var created = Assert.IsType<SceneCreatedEvent>(evt);
        Assert.Equal("scene-1", created.SceneId);
        Assert.Equal("session-1", created.SessionId);
        Assert.Equal("Forest Ambush", created.SceneName);
    }

    [Fact]
    public void CreateRejectsEmptySceneId()
    {
        Assert.Throws<ArgumentException>(() => SceneAggregate.Create(string.Empty, "session-1", "name", Now));
        Assert.Throws<ArgumentException>(() => SceneAggregate.Create("   ", "session-1", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptySessionId()
    {
        Assert.Throws<ArgumentException>(() => SceneAggregate.Create("scene-1", string.Empty, "name", Now));
        Assert.Throws<ArgumentException>(() => SceneAggregate.Create("scene-1", "   ", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptySceneName()
    {
        Assert.Throws<ArgumentException>(() => SceneAggregate.Create("scene-1", "session-1", string.Empty, Now));
    }

    [Fact]
    public void CloseTransitionsToClosed()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);

        aggregate.Close(Now.AddDays(1));

        Assert.Equal(SceneLifecycle.Closed, aggregate.Lifecycle);
        Assert.NotNull(aggregate.ClosedAt);
        Assert.Equal(Now.AddDays(1), aggregate.ClosedAt);
    }

    [Fact]
    public void CloseEmitsSceneClosedEvent()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);

        aggregate.Close(Now.AddDays(1));

        var evt = Assert.Single(aggregate.UncommittedEvents.OfType<SceneClosedEvent>());
        Assert.Equal("scene-1", evt.SceneId);
    }

    [Fact]
    public void CloseRejectsSecondClosure()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void MutationsAfterClosureAreRejected()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void CaptureStateRoundTripsThroughRehydrate()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);
        aggregate.Close(Now.AddDays(1));

        var state = aggregate.CaptureState();
        var rehydrated = SceneAggregate.Rehydrate(state);

        Assert.Equal("scene-1", rehydrated.SceneId);
        Assert.Equal("session-1", rehydrated.SessionId);
        Assert.Equal("Forest Ambush", rehydrated.SceneName);
        Assert.Equal(SceneLifecycle.Closed, rehydrated.Lifecycle);
        Assert.Equal(Now, rehydrated.CreatedAt);
        Assert.Equal(Now.AddDays(1), rehydrated.ClosedAt);
        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void RehydrateDoesNotEmitDomainEvents()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);
        aggregate.Close(Now.AddDays(1));
        var state = aggregate.CaptureState();

        var rehydrated = SceneAggregate.Rehydrate(state);

        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void DequeueUncommittedEventsEmptiesQueue()
    {
        var aggregate = SceneAggregate.Create("scene-1", "session-1", "Forest Ambush", Now);
        aggregate.Close(Now.AddDays(1));

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.NotEmpty(drained);
        Assert.Empty(aggregate.UncommittedEvents);
    }
}