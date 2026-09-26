using Chronicle.Domain.Campaign;
using Chronicle.Domain.Campaign.Events;
using Xunit;

namespace Chronicle.Domain.Tests.Campaign;

public sealed class CampaignAggregateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateInitializesCampaignIdAndName()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);

        Assert.Equal("campaign-1", aggregate.CampaignId);
        Assert.Equal("The Iron Chronicles", aggregate.CampaignName);
        Assert.Equal(CampaignLifecycle.Active, aggregate.Lifecycle);
        Assert.Equal(Now, aggregate.CreatedAt);
        Assert.Null(aggregate.ClosedAt);
    }

    [Fact]
    public void CreateEmitsCampaignCreatedEvent()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);

        var evt = Assert.Single(aggregate.UncommittedEvents);
        var created = Assert.IsType<CampaignCreatedEvent>(evt);
        Assert.Equal("campaign-1", created.CampaignId);
        Assert.Equal("The Iron Chronicles", created.CampaignName);
    }

    [Fact]
    public void CreateRejectsEmptyCampaignId()
    {
        Assert.Throws<ArgumentException>(() => CampaignAggregate.Create(string.Empty, "name", Now));
        Assert.Throws<ArgumentException>(() => CampaignAggregate.Create("   ", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptyCampaignName()
    {
        Assert.Throws<ArgumentException>(() => CampaignAggregate.Create("campaign-1", string.Empty, Now));
    }

    [Fact]
    public void CloseTransitionsToClosed()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);

        aggregate.Close(Now.AddDays(1));

        Assert.Equal(CampaignLifecycle.Closed, aggregate.Lifecycle);
        Assert.NotNull(aggregate.ClosedAt);
        Assert.Equal(Now.AddDays(1), aggregate.ClosedAt);
    }

    [Fact]
    public void CloseEmitsCampaignClosedEvent()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);

        aggregate.Close(Now.AddDays(1));

        var evt = Assert.Single(aggregate.UncommittedEvents.OfType<CampaignClosedEvent>());
        Assert.Equal("campaign-1", evt.CampaignId);
    }

    [Fact]
    public void CloseRejectsSecondClosure()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void MutationsAfterClosureAreRejected()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);
        aggregate.Close(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Close(Now.AddDays(2)));
    }

    [Fact]
    public void CaptureStateRoundTripsThroughRehydrate()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);
        aggregate.Close(Now.AddDays(1));

        var state = aggregate.CaptureState();
        var rehydrated = CampaignAggregate.Rehydrate(state);

        Assert.Equal("campaign-1", rehydrated.CampaignId);
        Assert.Equal("The Iron Chronicles", rehydrated.CampaignName);
        Assert.Equal(CampaignLifecycle.Closed, rehydrated.Lifecycle);
        Assert.Equal(Now, rehydrated.CreatedAt);
        Assert.Equal(Now.AddDays(1), rehydrated.ClosedAt);
        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void RehydrateDoesNotEmitDomainEvents()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);
        aggregate.Close(Now.AddDays(1));
        var state = aggregate.CaptureState();

        var rehydrated = CampaignAggregate.Rehydrate(state);

        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void DequeueUncommittedEventsEmptiesQueue()
    {
        var aggregate = CampaignAggregate.Create("campaign-1", "The Iron Chronicles", Now);
        aggregate.Close(Now.AddDays(1));

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.NotEmpty(drained);
        Assert.Empty(aggregate.UncommittedEvents);
    }
}