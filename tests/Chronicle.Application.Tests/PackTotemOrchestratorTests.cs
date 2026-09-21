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
    private static readonly string[] NoMembersLocal = Array.Empty<string>();
    private static readonly string[] NoImprovementsLocal = Array.Empty<string>();
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

    [Fact]
    public async Task FindByPackIdAsyncReturnsGuidForExistingPack()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));

        var resolved = await orchestrator.FindByPackIdAsync("pack-1");

        Assert.NotNull(resolved);
        Assert.Equal(harness.GetAggregateId("pack-1"), resolved.Value);
    }

    [Fact]
    public async Task FindByPackIdAsyncReturnsNullForUnknownPack()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);

        var resolved = await orchestrator.FindByPackIdAsync("never-created");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task FindByPackIdAsyncIgnoresNonPackDocuments()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        var unrelatedState = new PackTotemState(
            PackId: "unrelated-pack",
            PackName: "Unrelated",
            Members: NoMembersLocal,
            LeaderId: null,
            TotemId: null,
            TotemRating: 0,
            TotemImprovementPurchases: NoImprovementsLocal,
            LinkState: PackTotemLinkState.Unbound,
            ActiveTactics: NoMembersLocal,
            LastTotemXpResolution: TotemXpResolutionState.Unresolved,
            EstablishedAt: Now,
            DissolvedAt: null);
        var unrelatedDocument = new Document(
            Guid.NewGuid(),
            "some-other-content-type/v1",
            PackTotemSerializer.Serialize(unrelatedState),
            Version: 0);
        await harness.Store.SaveAsync(unrelatedDocument, expectedVersion: null);

        var resolved = await orchestrator.FindByPackIdAsync("unrelated-pack");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task FindByPackIdAsyncPropagatesDeserializationFailureForMalformedPackDocument()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        harness.InjectMalformedPackDocument("malformed-pack", "this is not valid json");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() =>
            orchestrator.FindByPackIdAsync("any-pack"));
    }

    [Fact]
    public async Task AddMemberAsyncAddsAndPersists()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));

        var result = await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));

        Assert.True(result.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Single(reloaded.Members);
        Assert.Contains("alpha", reloaded.Members);
        Assert.Equal(2, harness.GetVersion("pack-1"));
    }

    [Fact]
    public async Task AddMemberAsyncFailsForUnknownPack()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);

        var result = await orchestrator.AddMemberAsync(new AddMemberRequest("missing-pack", "alpha"));

        Assert.False(result.Succeeded);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task AddMemberAsyncFailsForDuplicateMember()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        var first = await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));
        Assert.True(first.Succeeded);

        var second = await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));

        Assert.False(second.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Single(reloaded.Members);
    }

    [Fact]
    public async Task RemoveMemberAsyncRemovesAndPersists()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "beta"));
        var versionAfterAdds = harness.GetVersion("pack-1");

        var result = await orchestrator.RemoveMemberAsync(new RemoveMemberRequest("pack-1", "alpha"));

        Assert.True(result.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Single(reloaded.Members);
        Assert.DoesNotContain("alpha", reloaded.Members);
        Assert.Contains("beta", reloaded.Members);
        Assert.True(harness.GetVersion("pack-1") > versionAfterAdds);
    }

    [Fact]
    public async Task RemoveMemberAsyncClearsLeaderWhenRemovingLeader()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));
        var setLeader = await orchestrator.SetLeaderAsync(new SetLeaderRequest("pack-1", "alpha"));
        Assert.True(setLeader.Succeeded);

        var result = await orchestrator.RemoveMemberAsync(new RemoveMemberRequest("pack-1", "alpha"));

        Assert.True(result.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Empty(reloaded.Members);
        Assert.Null(reloaded.LeaderId);
    }

    [Fact]
    public async Task SetLeaderAsyncSucceedsForCurrentMember()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));

        var result = await orchestrator.SetLeaderAsync(new SetLeaderRequest("pack-1", "alpha"));

        Assert.True(result.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Equal("alpha", reloaded.LeaderId);
    }

    [Fact]
    public async Task SetLeaderAsyncFailsForNonMember()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));

        var result = await orchestrator.SetLeaderAsync(new SetLeaderRequest("pack-1", "stranger"));

        Assert.False(result.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Null(reloaded.LeaderId);
    }

    [Fact]
    public async Task DissolveAsyncSucceedsAndBlocksFurtherMutations()
    {
        var harness = new InMemoryPackTotemHarness();
        var orchestrator = new PackTotemOrchestrator(harness.Store);
        await orchestrator.CreatePackAsync(new CreatePackRequest("pack-1", "Iron Wolves", Now));
        await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "alpha"));

        var dissolve = await orchestrator.DissolveAsync(new DissolveRequest("pack-1", Now));

        Assert.True(dissolve.Succeeded);
        var reloaded = harness.GetState("pack-1");
        Assert.Equal(PackTotemLinkState.Dissolving, reloaded.LinkState);
        Assert.NotNull(reloaded.DissolvedAt);

        var bindAttempt = await orchestrator.BindTotemAsync(new BindTotemRequest(
            harness.GetAggregateId("pack-1"),
            "pack-1",
            "wolf-totem",
            3,
            1,
            Array.Empty<string>()));
        Assert.False(bindAttempt.Succeeded);
        Assert.Contains("dissolved", bindAttempt.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        var addAttempt = await orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", "beta"));
        Assert.False(addAttempt.Succeeded);
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

        public void InjectMalformedPackDocument(string packIdKey, string payloadJson)
        {
            var document = new Document(
                Guid.NewGuid(),
                PackTotemSerializer.ContentType,
                payloadJson,
                Version: 0);
            documents[packIdKey] = document;
            ids[packIdKey] = document.Id;
        }
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

        public Task<IReadOnlyList<Document>> EnumerateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Document>>(documents.Values.ToArray());
        }
    }
}
