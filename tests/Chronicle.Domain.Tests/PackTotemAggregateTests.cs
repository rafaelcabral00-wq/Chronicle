using Chronicle.Domain.PackTotem;
using Chronicle.Domain.PackTotem.Events;
using Xunit;

namespace Chronicle.Domain.Tests;

public sealed class PackTotemAggregateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateInitializesPackIdAndName()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        Assert.Equal("pack-1", aggregate.PackId);
        Assert.Equal("The Iron Wolves", aggregate.PackName);
        Assert.Empty(aggregate.Members);
        Assert.Null(aggregate.TotemId);
        Assert.Equal(PackTotemLinkState.Unbound, aggregate.LinkState);
        Assert.Equal(0, aggregate.TotemRating);
        Assert.Equal(TotemXpResolutionState.Unresolved, aggregate.LastTotemXpResolution);
        Assert.False(aggregate.IsDissolved);
    }

    [Fact]
    public void CreateEmitsPackCreatedEvent()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        var evt = Assert.Single(aggregate.UncommittedEvents);
        var created = Assert.IsType<PackCreatedEvent>(evt);
        Assert.Equal("pack-1", created.PackId);
        Assert.Equal("The Iron Wolves", created.PackName);
    }

    [Fact]
    public void CreateRejectsEmptyPackId()
    {
        Assert.Throws<ArgumentException>(() => PackTotemAggregate.Create(string.Empty, "name", Now));
        Assert.Throws<ArgumentException>(() => PackTotemAggregate.Create("   ", "name", Now));
    }

    [Fact]
    public void CreateRejectsEmptyPackName()
    {
        Assert.Throws<ArgumentException>(() => PackTotemAggregate.Create("pack-1", string.Empty, Now));
    }

    [Fact]
    public void AddMemberAddsToRoster()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.AddMember("char-1");

        Assert.Single(aggregate.Members);
        Assert.Contains("char-1", aggregate.Members);
    }

    [Fact]
    public void AddMemberEmitsMemberJoinedEvent()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.AddMember("char-1");

        var evt = aggregate.UncommittedEvents.Single(e => e is PackMemberJoinedEvent);
        var joined = Assert.IsType<PackMemberJoinedEvent>(evt);
        Assert.Equal("char-1", joined.CharacterId);
        Assert.Equal("pack-1", joined.PackId);
    }

    [Fact]
    public void AddMemberRejectsDuplicate()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");

        Assert.Throws<InvalidOperationException>(() => aggregate.AddMember("char-1"));
    }

    [Fact]
    public void AddMemberRejectsEmptyCharacterId()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        Assert.Throws<ArgumentException>(() => aggregate.AddMember(string.Empty));
    }

    [Fact]
    public void RemoveMemberRemovesFromRoster()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");
        aggregate.AddMember("char-2");

        aggregate.RemoveMember("char-1");

        Assert.Single(aggregate.Members);
        Assert.DoesNotContain("char-1", aggregate.Members);
    }

    [Fact]
    public void RemoveMemberEmitsMemberLeftEvent()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");

        aggregate.RemoveMember("char-1");

        var evt = aggregate.UncommittedEvents.Single(e => e is PackMemberLeftEvent);
        var left = Assert.IsType<PackMemberLeftEvent>(evt);
        Assert.Equal("char-1", left.CharacterId);
    }

    [Fact]
    public void RemoveMemberRejectsNonMember()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");

        Assert.Throws<InvalidOperationException>(() => aggregate.RemoveMember("char-2"));
    }

    [Fact]
    public void RemoveMemberClearsLeaderIfRemoved()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");
        aggregate.SetLeader("char-1");

        aggregate.RemoveMember("char-1");

        Assert.Null(aggregate.LeaderId);
    }

    [Fact]
    public void SetLeaderSetsExistingMemberAsLeader()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");

        aggregate.SetLeader("char-1");

        Assert.Equal("char-1", aggregate.LeaderId);
    }

    [Fact]
    public void SetLeaderRejectsNonMember()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        Assert.Throws<InvalidOperationException>(() => aggregate.SetLeader("char-99"));
    }

    [Fact]
    public void BindTotemTransitionsToBound()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");

        aggregate.BindTotem("falcon", 3, 7, initialImprovementPurchases: ["communal-senses"]);

        Assert.Equal("falcon", aggregate.TotemId);
        Assert.Equal(3, aggregate.TotemRating);
        Assert.Equal(PackTotemLinkState.Bound, aggregate.LinkState);
        Assert.Contains("communal-senses", aggregate.TotemImprovementPurchases);
    }

    [Fact]
    public void BindTotemEmitsTotemBoundEvent()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.BindTotem("falcon", 3, 7, initialImprovementPurchases: []);

        var evt = Assert.Single(aggregate.UncommittedEvents.OfType<TotemBoundEvent>());
        Assert.Equal("falcon", evt.TotemId);
        Assert.Equal(3, evt.TotemRating);
        Assert.Equal(7, evt.TotemAggregation);
    }

    [Fact]
    public void BindTotemKeepsA012Unresolved()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.BindTotem("falcon", 3, 7, initialImprovementPurchases: []);

        Assert.Equal(TotemXpResolutionState.Unresolved, aggregate.LastTotemXpResolution);
    }

    [Fact]
    public void BindTotemRejectsDoubleBinding()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.BindTotem("falcon", 3, 7, initialImprovementPurchases: []);

        Assert.Throws<InvalidOperationException>(() =>
            aggregate.BindTotem("wolf", 2, 4, initialImprovementPurchases: []));
    }

    [Fact]
    public void BindTotemRejectsEmptyTotemId()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        Assert.Throws<ArgumentException>(() =>
            aggregate.BindTotem(string.Empty, 1, 1, initialImprovementPurchases: []));
    }

    [Fact]
    public void BindTotemRejectsNonPositiveRating()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            aggregate.BindTotem("falcon", 0, 1, initialImprovementPurchases: []));
    }

    [Fact]
    public void DissolveTransitionsToDissolved()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.Dissolve(Now.AddDays(1));

        Assert.True(aggregate.IsDissolved);
        Assert.Equal(PackTotemLinkState.Dissolving, aggregate.LinkState);
        Assert.NotNull(aggregate.DissolvedAt);
    }

    [Fact]
    public void DissolveEmitsPackDissolvedEvent()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);

        aggregate.Dissolve(Now.AddDays(1));

        var evt = Assert.Single(aggregate.UncommittedEvents.OfType<PackDissolvedEvent>());
        Assert.Equal("pack-1", evt.PackId);
    }

    [Fact]
    public void DissolveRejectsSecondDissolution()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.Dissolve(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Dissolve(Now.AddDays(2)));
    }

    [Fact]
    public void MutationsAfterDissolutionAreRejected()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.Dissolve(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.AddMember("char-1"));
        Assert.Throws<InvalidOperationException>(() => aggregate.RemoveMember("char-1"));
        Assert.Throws<InvalidOperationException>(() => aggregate.SetLeader("char-1"));
        Assert.Throws<InvalidOperationException>(() =>
            aggregate.BindTotem("falcon", 1, 1, initialImprovementPurchases: []));
    }

    [Fact]
    public void CaptureStateRoundTripsThroughRehydrate()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");
        aggregate.AddMember("char-2");
        aggregate.SetLeader("char-1");
        aggregate.BindTotem("falcon", 3, 7, initialImprovementPurchases: ["a", "b"]);

        var state = aggregate.CaptureState();
        var rehydrated = PackTotemAggregate.Rehydrate(state);

        Assert.Equal("pack-1", rehydrated.PackId);
        Assert.Equal("The Iron Wolves", rehydrated.PackName);
        Assert.Equal(2, rehydrated.Members.Count);
        Assert.Contains("char-1", rehydrated.Members);
        Assert.Contains("char-2", rehydrated.Members);
        Assert.Equal("char-1", rehydrated.LeaderId);
        Assert.Equal("falcon", rehydrated.TotemId);
        Assert.Equal(3, rehydrated.TotemRating);
        Assert.Equal(2, rehydrated.TotemImprovementPurchases.Count);
        Assert.Equal(PackTotemLinkState.Bound, rehydrated.LinkState);
        Assert.Equal(Now, rehydrated.EstablishedAt);
        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void RehydrateDoesNotEmitDomainEvents()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");
        var state = aggregate.CaptureState();

        var rehydrated = PackTotemAggregate.Rehydrate(state);

        Assert.Empty(rehydrated.UncommittedEvents);
    }

    [Fact]
    public void DequeueUncommittedEventsEmptiesQueue()
    {
        var aggregate = PackTotemAggregate.Create("pack-1", "The Iron Wolves", Now);
        aggregate.AddMember("char-1");
        aggregate.BindTotem("falcon", 1, 1, initialImprovementPurchases: []);

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.NotEmpty(drained);
        Assert.Empty(aggregate.UncommittedEvents);
    }
}
