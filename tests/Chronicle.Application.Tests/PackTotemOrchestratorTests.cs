using System.Text.Json;
using Chronicle.Application.PackTotem;
using Chronicle.Application.Persistence;
using Chronicle.Domain.PackTotem;
using Xunit;
#pragma warning disable CA1861

namespace Chronicle.Application.Tests;

public sealed class PackTotemSerializerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly string[] OneMember = new[] { "char-1" };
    private static readonly string[] TwoMembers = new[] { "char-1", "char-2" };
    private static readonly string[] OneImprovement = new[] { "a" };
    private static readonly string[] TwoImprovements = new[] { "a", "b" };
    private static readonly string[] OneTactic = new[] { "tactic-1" };
    private static readonly string[] NoMembers = Array.Empty<string>();
    private static readonly string[] NoImprovements = Array.Empty<string>();

    [Fact]
    public void SerializeProducesCamelCaseJson()
    {
        var state = new PackTotemState(
            PackId: "pack-1", PackName: "Iron Wolves", Members: OneMember,
            LeaderId: "char-1", TotemId: "falcon", TotemRating: 3,
            TotemImprovementPurchases: OneImprovement,
            LinkState: PackTotemLinkState.Bound, ActiveTactics: NoMembers,
            LastTotemXpResolution: TotemXpResolutionState.Unresolved,
            EstablishedAt: Now, DissolvedAt: null);

        var json = PackTotemSerializer.Serialize(state);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("pack-1", doc.RootElement.GetProperty("packId").GetString());
        Assert.Equal("bound", doc.RootElement.GetProperty("linkState").GetString());
    }

    [Fact]
    public void SerializeKeepsA012Unresolved()
    {
        var state = new PackTotemState(
            PackId: "pack-1", PackName: "Iron Wolves", Members: NoMembers,
            LeaderId: null, TotemId: "falcon", TotemRating: 3,
            TotemImprovementPurchases: NoImprovements,
            LinkState: PackTotemLinkState.Bound, ActiveTactics: NoMembers,
            LastTotemXpResolution: TotemXpResolutionState.Unresolved,
            EstablishedAt: Now, DissolvedAt: null);

        var json = PackTotemSerializer.Serialize(state);
        var deserialized = PackTotemSerializer.Deserialize(json);
        Assert.Equal(TotemXpResolutionState.Unresolved, deserialized.LastTotemXpResolution);
    }

    [Fact]
    public void DeserializeRoundTripsAllFields()
    {
        var state = new PackTotemState(
            PackId: "pack-1", PackName: "Iron Wolves", Members: TwoMembers,
            LeaderId: "char-1", TotemId: "falcon", TotemRating: 5,
            TotemImprovementPurchases: TwoImprovements,
            LinkState: PackTotemLinkState.Bound, ActiveTactics: OneTactic,
            LastTotemXpResolution: TotemXpResolutionState.Unresolved,
            EstablishedAt: Now, DissolvedAt: null);

        var json = PackTotemSerializer.Serialize(state);
        var deserialized = PackTotemSerializer.Deserialize(json);

        Assert.Equal(state.PackId, deserialized.PackId);
        Assert.Equal(state.PackName, deserialized.PackName);
        Assert.Equal(state.Members, deserialized.Members);
        Assert.Equal(state.LeaderId, deserialized.LeaderId);
        Assert.Equal(state.TotemId, deserialized.TotemId);
        Assert.Equal(state.TotemRating, deserialized.TotemRating);
        Assert.Equal(state.TotemImprovementPurchases, deserialized.TotemImprovementPurchases);
        Assert.Equal(state.LinkState, deserialized.LinkState);
        Assert.Equal(state.ActiveTactics, deserialized.ActiveTactics);
        Assert.Equal(state.LastTotemXpResolution, deserialized.LastTotemXpResolution);
        Assert.Equal(state.EstablishedAt, deserialized.EstablishedAt);
        Assert.Equal(state.DissolvedAt, deserialized.DissolvedAt);
    }

    [Fact]
    public void SerializeRejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() => PackTotemSerializer.Serialize(null!));
    }

    [Fact]
    public void DeserializeRejectsEmptyPayload()
    {
        Assert.Throws<ArgumentException>(() => PackTotemSerializer.Deserialize(string.Empty));
    }
}

public sealed class PackTotemOrchestratorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly string[] NoPurchases = Array.Empty<string>();
    private static readonly string[] TwoImprovements = new[] { "communal-senses", "pack-speech" };
    private static readonly string[] SingleImprovement = new[] { "communal-senses" };

    [Fact]
    public async Task CreatePackPersistsAggregateAtVersionOne()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);

        var result = await orchestrator.CreatePackAsync(
            new CreatePackRequest("pack-1", "Iron Wolves", Now));

        Assert.True(result.Succeeded);
        Assert.Equal("pack-1", result.PackId);
        Assert.Equal(PackTotemLinkState.Unbound, result.LinkState);
        Assert.Equal(1, harness.GetVersion("pack-1"));
    }

    [Fact]
    public async Task BindTotemTransitionsAggregateToBound()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        var create = await orchestrator.CreatePackAsync(
            new CreatePackRequest("pack-1", "Iron Wolves", Now));
        Assert.True(create.Succeeded);

        var aggregateId = harness.GetAggregateId("pack-1");
        var bindResult = await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 3, 7, SingleImprovement));

        Assert.True(bindResult.Succeeded);
        Assert.Equal(PackTotemLinkState.Bound, bindResult.LinkState);

        var reloadedState = harness.GetState("pack-1");
        Assert.Equal("falcon", reloadedState.TotemId);
        Assert.Equal(3, reloadedState.TotemRating);
        Assert.Contains("communal-senses", reloadedState.TotemImprovementPurchases);
    }

    [Fact]
    public async Task BindTotemFailsForUnknownPack()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);

        var result = await orchestrator.BindTotemAsync(new BindTotemRequest(
            Guid.NewGuid(), "missing-pack", "falcon", 1, 1, NoPurchases));

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindTotemFailsWhenPackAlreadyBound()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var aggregateId = harness.GetAggregateId("pack-1");
        await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 3, 7, NoPurchases));

        var second = await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "wolf", 2, 4, NoPurchases));

        Assert.False(second.Succeeded);
        Assert.Contains("already bound", second.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindTotemDoesNotSilentlyResolveA012()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var aggregateId = harness.GetAggregateId("pack-1");

        await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 3, 7, NoPurchases));

        var state = harness.GetState("pack-1");
        Assert.Equal(TotemXpResolutionState.Unresolved, state.LastTotemXpResolution);
    }

    [Fact]
    public async Task BindTotemDoesNotPublishDomainEvents()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var aggregateId = harness.GetAggregateId("pack-1");

        var result = await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 3, 7, NoPurchases));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task PersistenceFailureIsNotReportedAsSuccess()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);

        var result = await orchestrator.BindTotemAsync(new BindTotemRequest(
            Guid.NewGuid(), "missing-pack", "falcon", 1, 1, NoPurchases));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task StateSurvivesRehydrationThroughDocument()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var aggregateId = harness.GetAggregateId("pack-1");
        await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 5, 8, TwoImprovements));

        var state = harness.GetState("pack-1");
        Assert.Equal("pack-1", state.PackId);
        Assert.Equal("Iron Wolves", state.PackName);
        Assert.Equal("falcon", state.TotemId);
        Assert.Equal(5, state.TotemRating);
        Assert.Equal(PackTotemLinkState.Bound, state.LinkState);
        Assert.Equal(2, state.TotemImprovementPurchases.Count);
    }

    [Fact]
    public async Task DocumentVersionIncrementsAcrossOperations()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var aggregateId = harness.GetAggregateId("pack-1");
        await orchestrator.BindTotemAsync(new BindTotemRequest(
            aggregateId, "pack-1", "falcon", 3, 7, NoPurchases));

        Assert.Equal(2, harness.GetVersion("pack-1"));
    }

    private sealed class InMemoryPackTotemHarness
    {
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public AggregateStore Store { get; }

        public InMemoryPackTotemHarness()
        {
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
        }

        public Guid GetAggregateId(string packId) => ids[packId];

        public long GetVersion(string packId) => documents[packId].Version;

        public PackTotemState GetState(string packId) =>
            PackTotemSerializer.Deserialize(documents[packId].PayloadJson);
    }

    private sealed class InMemoryDocumentRepository : IDocumentRepository
    {
        private readonly Dictionary<string, Document> documents;
        private readonly Dictionary<string, Guid> ids;

        public InMemoryDocumentRepository(
            Dictionary<string, Document> documents,
            Dictionary<string, Guid> ids)
        {
            this.documents = documents;
            this.ids = ids;
        }

        public Task<DocumentPersistenceResult> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            foreach (var document in documents.Values)
            {
                if (document.Id == id)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, document, null));
                }
            }
            return Task.FromResult(new DocumentPersistenceResult(
                DocumentPersistenceStatus.NotFound, null, null));
        }

        public Task<DocumentPersistenceResult> SaveAsync(
            Document document,
            long? expectedVersion,
            CancellationToken cancellationToken = default)
        {
            var state = PackTotemSerializer.Deserialize(document.PayloadJson);
            if (documents.TryGetValue(state.PackId, out var existing))
            {
                if (expectedVersion is null)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict, null, "Already exists."));
                }
                if (expectedVersion.Value != existing.Version)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict, null, "Version mismatch."));
                }
                var updated = document with { Version = existing.Version + 1 };
                documents[state.PackId] = updated;
                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.Succeeded, updated, null));
            }
            else
            {
                if (expectedVersion is not null)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict, null, "Expected version but absent."));
                }
                var created = document with { Version = 1 };
                documents[state.PackId] = created;
                ids[state.PackId] = created.Id;
                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.Succeeded, created, null));
            }
        }

        public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            foreach (var (packId, document) in documents.ToArray())
            {
                if (document.Id == id)
                {
                    documents.Remove(packId);
                    ids.Remove(packId);
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, null, null));
                }
            }
            return Task.FromResult(new DocumentPersistenceResult(
                DocumentPersistenceStatus.NotFound, null, null));
        }
    }
}
