using Xunit;

namespace Chronicle.Domain.Tests;

public sealed class AggregateRootFoundationTests
{
    private sealed class SampleCreatedEvent : IDomainEvent
    {
        public string Name { get; }
        public SampleCreatedEvent(string name) { Name = name; }
    }

    private sealed class SampleRenamedEvent : IDomainEvent
    {
        public string NewName { get; }
        public SampleRenamedEvent(string newName) { NewName = newName; }
    }

    private sealed class SampleAggregate : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;

        public void Create(string name)
        {
            Name = name;
            RecordEvent(new SampleCreatedEvent(name));
        }

        public void Rename(string newName)
        {
            Name = newName;
            RecordEvent(new SampleRenamedEvent(newName));
        }
    }

    [Fact]
    public void AggregateExposesStableIdentity()
    {
        var aggregate = new SampleAggregate();

        Assert.NotEqual(Guid.Empty, aggregate.Id);
    }

    [Fact]
    public void TwoAggregatesHaveDistinctIdentities()
    {
        var a = new SampleAggregate();
        var b = new SampleAggregate();

        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void AggregateWithProvidedIdentityPreservesIt()
    {
        var id = Guid.NewGuid();
        var aggregate = new IdentityCapturingAggregate(id);

        Assert.Equal(id, aggregate.Id);
    }

    private sealed class IdentityCapturingAggregate : AggregateRoot
    {
        public IdentityCapturingAggregate(Guid id) : base(id) { }
    }

    [Fact]
    public void UncommittedEventsExposeRecordedEventsReadOnly()
    {
        var aggregate = new SampleAggregate();
        aggregate.Create("alpha");
        aggregate.Rename("beta");

        var snapshot = aggregate.UncommittedEvents;

        Assert.Equal(2, snapshot.Count);
        Assert.IsType<SampleCreatedEvent>(snapshot[0]);
        Assert.IsType<SampleRenamedEvent>(snapshot[1]);
        Assert.Equal("alpha", ((SampleCreatedEvent)snapshot[0]).Name);
        Assert.Equal("beta", ((SampleRenamedEvent)snapshot[1]).NewName);
    }

    [Fact]
    public void UncommittedEventsSnapshotIsIndependentSnapshot()
    {
        var aggregate = new SampleAggregate();
        aggregate.Create("alpha");

        var first = aggregate.UncommittedEvents;
        aggregate.Rename("beta");
        var second = aggregate.UncommittedEvents;

        Assert.Single(first);
        Assert.Equal(2, second.Count);
    }

    [Fact]
    public void RecordEventRejectsNull()
    {
        var aggregate = new SampleAggregate();
        var method = typeof(AggregateRoot)
            .GetMethod("RecordEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(method);
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            method!.Invoke(aggregate, new object?[] { null }));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void DequeueUncommittedEventsReturnsAllAndClearsQueue()
    {
        var aggregate = new SampleAggregate();
        aggregate.Create("alpha");
        aggregate.Rename("beta");

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.Equal(2, drained.Count);
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public void DequeueOnEmptyAggregateReturnsEmpty()
    {
        var aggregate = new SampleAggregate();

        var drained = aggregate.DequeueUncommittedEvents();

        Assert.Empty(drained);
    }

    [Fact]
    public void NewEventsAfterDequeueAreTreatedAsFreshUncommitted()
    {
        var aggregate = new SampleAggregate();
        aggregate.Create("alpha");
        aggregate.DequeueUncommittedEvents();
        aggregate.Rename("beta");

        var snapshot = aggregate.UncommittedEvents;

        Assert.Single(snapshot);
        Assert.IsType<SampleRenamedEvent>(snapshot[0]);
    }

    [Fact]
    public void AggregateRootImplementsIAggregateRoot()
    {
        var aggregate = new SampleAggregate();

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }
}
