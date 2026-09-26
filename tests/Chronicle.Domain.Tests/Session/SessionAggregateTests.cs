using Chronicle.Domain.Session;
using Chronicle.Domain.Session.Events;
using Xunit;

namespace Chronicle.Domain.Tests.Session;

public sealed class SessionAggregateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateInitializesSessionIdCampaignIdAndName()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);

        Assert.Equal("session-1", aggregate.SessionId);
        Assert.Equal("campaign-1", aggregate.CampaignId);
        Assert.Equal("First Session", aggregate.SessionName);
        Assert.Equal(SessionLifecycle.Active, aggregate.Lifecycle);
        Assert.Equal(Now, aggregate.CreatedAt);
        Assert.Null(aggregate.ClosedAt);
    }

    [Fact]
    public void CreateEmitsSessionCreatedEvent()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);

        var evt = Assert.Single(aggregate.UncommittedEvents);
        var created = Assert.IsType<SessionCreatedEvent>(evt);
        Assert.Equal("session-1", created.SessionId);
        Assert.Equal("campaign-1", created.CampaignId);
        Assert.Equal("First Session", created.SessionName);
    }

    [Fact]
    public void CreateRejectsEmptySessionId()
    {
        Assert.Throws<ArgumentException>(() => SessionAggregate.Create(string.Empty, "campaign-1", "name", Now));
        Assert.Throws<ArgumentException>(() => SessionAggregate.Create("   ", "campaign-1", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptyCampaignId()
    {
        Assert.Throws<ArgumentException>(() => SessionAggregate.Create("session-1", string.Empty, "name", Now));
        Assert.Throws<ArgumentException>(() => SessionAggregate.Create("session-1", "   ", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptySessionName()
    {
        Assert.Throws<ArgumentException>(() => SessionAggregate.Create("session-1", "campaign-1", string.Empty, Now));
    }

    [Fact]
    public void CloseTransitionsToClosed()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);

        aggregate.Close(Now.AddDays(1));

        Assert.Equal(SessionLifecycle.Closed, aggregate.Lifecycle);
        Assert.NotNull(aggregate.ClosedAt);
        Assert.Equal(Now.AddDays(1), aggregate.ClosedAt);
    }

    [Fact]
    public void CloseEmitsSessionClosedEvent()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);

        aggregate.Close(Now.AddDays(1));

        var evt = Assert.Single(aggregate.UncommittedEvents.OfType<SessionClosedEvent>());
        Assert.Equal("session-1", evt.SessionId);
    }

    [Fact]
    public void CloseRejectsSecondClosure()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void MutationsAfterClosureAreRejected()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void CaptureStateRoundTripsThroughRehydrate()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);
        aggregate.Close(Now.AddDays(1));

        var state = aggregate.CaptureState();
        var rehydrated = SessionAggregate.Rehydrate(state);

        Assert.Equal("session-1", rehydrated.SessionId);
        Assert.Equal("campaign-1", rehydrated.CampaignId);
        Assert.Equal("First Session", rehydrated.SessionName);
        Assert.Equal(SessionLifecycle.Closed, rehydrated.Lifecycle);
        Assert.Equal(Now, rehydrated.CreatedAt);
        Assert.Equal(Now.AddDays(1), rehydrated.ClosedAt);
        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void RehydrateDoesNotEmitDomainEvents()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);
        aggregate.Close(Now.AddDays(1));
        var state = aggregate.CaptureState();

        var rehydrated = SessionAggregate.Rehydrate(state);

        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void DequeueUncommittedEventsEmptiesQueue()
    {
        var aggregate = SessionAggregate.Create("session-1", "campaign-1", "First Session", Now);
        aggregate.Close(Now.AddDays(1));

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.NotEmpty(drained);
        Assert.Empty(aggregate.UncommittedEvents);
    }
}