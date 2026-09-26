using Chronicle.Application.Campaign;
using Chronicle.Application.Persistence;
using Chronicle.Domain.Campaign;
using Xunit;

namespace Chronicle.Application.Tests.Campaign;

public sealed class CampaignOrchestratorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateCampaignPersistsAggregateAtVersionOne()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));

        Assert.True(result.Succeeded);
        Assert.Equal("campaign-1", result.CampaignId);
        Assert.Equal(CampaignLifecycle.Active, result.Lifecycle);
        Assert.Equal(1, harness.GetVersion("campaign-1"));
    }

    [Fact]
    public async Task CreateCampaignRejectsEmptyCampaignId()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest(string.Empty, "The Iron Chronicles", Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCampaignRejectsEmptyCampaignName()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", string.Empty, Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseCampaignTransitionsToClosed()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        var create = await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        Assert.True(create.Succeeded);

        var result = await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        Assert.True(result.Succeeded);
        Assert.Equal(CampaignLifecycle.Closed, result.Lifecycle);

        var reloadedState = harness.GetState("campaign-1");
        Assert.Equal(CampaignLifecycle.Closed, reloadedState.Lifecycle);
        Assert.Equal(Later, reloadedState.ClosedAt);
    }

    [Fact]
    public async Task CloseCampaignFailsForUnknownCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("missing-campaign", Later));

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseCampaignFailsWhenAlreadyClosed()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        var second = await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later.AddDays(1)));

        Assert.False(second.Succeeded);
        Assert.Contains("closed", second.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCampaignAsyncReturnsCampaignForExistingCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));

        var result = await orchestrator.GetCampaignAsync("campaign-1");

        Assert.True(result.Found);
        Assert.Equal("campaign-1", result.CampaignId);
        Assert.Equal("The Iron Chronicles", result.CampaignName);
        Assert.Equal(CampaignLifecycle.Active, result.Lifecycle);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task GetCampaignAsyncReturnsClosedCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        var result = await orchestrator.GetCampaignAsync("campaign-1");

        Assert.True(result.Found);
        Assert.Equal(CampaignLifecycle.Closed, result.Lifecycle);
        Assert.Equal(Later, result.ClosedAt);
    }

    [Fact]
    public async Task GetCampaignAsyncReturnsNotFoundForUnknownCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.GetCampaignAsync("never-created");

        Assert.False(result.Found);
        Assert.Null(result.CampaignId);
    }

    [Fact]
    public async Task GetCampaignAsyncIgnoresNonCampaignDocuments()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        var unrelatedState = new CampaignState(
            CampaignId: "unrelated-campaign",
            CampaignName: "Unrelated",
            Lifecycle: CampaignLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);
        var unrelatedDocument = new Document(
            Guid.NewGuid(),
            "some-other-content-type/v1",
            CampaignSerializer.Serialize(unrelatedState),
            Version: 0);
        await harness.Store.SaveAsync(unrelatedDocument, expectedVersion: null);

        var result = await orchestrator.GetCampaignAsync("unrelated-campaign");

        Assert.False(result.Found);
    }

    [Fact]
    public async Task GetCampaignAsyncPropagatesDeserializationFailureForMalformedCampaignDocument()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        harness.InjectMalformedCampaignDocument("malformed-campaign", "this is not valid json");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() =>
            orchestrator.GetCampaignAsync("any-campaign"));
    }

    [Fact]
    public async Task StateSurvivesRehydrationThroughDocument()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        var state = harness.GetState("campaign-1");
        Assert.Equal("campaign-1", state.CampaignId);
        Assert.Equal("The Iron Chronicles", state.CampaignName);
        Assert.Equal(CampaignLifecycle.Closed, state.Lifecycle);
        Assert.Equal(Now, state.CreatedAt);
        Assert.Equal(Later, state.ClosedAt);
    }

    [Fact]
    public async Task DocumentVersionIncrementsAcrossOperations()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        Assert.Equal(2, harness.GetVersion("campaign-1"));
    }

    [Fact]
    public async Task FindByCampaignIdAsyncReturnsGuidForExistingCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));

        var resolved = await orchestrator.FindByCampaignIdAsync("campaign-1");

        Assert.NotNull(resolved);
        Assert.Equal(harness.GetAggregateId("campaign-1"), resolved.Value);
    }

    [Fact]
    public async Task FindByCampaignIdAsyncReturnsNullForUnknownCampaign()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var resolved = await orchestrator.FindByCampaignIdAsync("never-created");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task VersionIncrementsOnMutate()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);
        await orchestrator.CreateCampaignAsync(
            new CreateCampaignRequest("campaign-1", "The Iron Chronicles", Now));
        Assert.Equal(1, harness.GetVersion("campaign-1"));

        await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("campaign-1", Later));

        Assert.Equal(2, harness.GetVersion("campaign-1"));
    }

    [Fact]
    public async Task PersistenceFailureIsNotReportedAsSuccess()
    {
        var harness = new InMemoryCampaignHarness();
        var orchestrator = new CampaignOrchestrator(harness.Store);

        var result = await orchestrator.CloseCampaignAsync(
            new CloseCampaignRequest("missing-campaign", Later));

        Assert.False(result.Succeeded);
    }

    private sealed class InMemoryCampaignHarness
    {
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public AggregateStore Store { get; }

        public InMemoryCampaignHarness()
        {
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
        }

        public Guid GetAggregateId(string campaignId) => ids[campaignId];

        public long GetVersion(string campaignId) => documents[campaignId].Version;

        public CampaignState GetState(string campaignId) =>
            CampaignSerializer.Deserialize(documents[campaignId].PayloadJson);

        public void InjectMalformedCampaignDocument(string campaignIdKey, string payloadJson)
        {
            var document = new Document(
                Guid.NewGuid(),
                CampaignSerializer.ContentType,
                payloadJson,
                Version: 0);
            documents[campaignIdKey] = document;
            ids[campaignIdKey] = document.Id;
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
                var state = CampaignSerializer.Deserialize(document.PayloadJson);
                if (documents.TryGetValue(state.CampaignId, out var existing))
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
                    documents[state.CampaignId] = updated;
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
                    documents[state.CampaignId] = created;
                    ids[state.CampaignId] = created.Id;
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, created, null));
                }
            }

            public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                foreach (var (campaignId, document) in documents.ToArray())
                {
                    if (document.Id == id)
                    {
                        documents.Remove(campaignId);
                        ids.Remove(campaignId);
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
}