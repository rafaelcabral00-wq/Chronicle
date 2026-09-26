using Chronicle.Application.Scene;
using Chronicle.Application.Persistence;
using Chronicle.Domain.Scene;
using Xunit;

namespace Chronicle.Application.Tests.Scene;

public sealed class SceneOrchestratorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateScenePersistsAggregateAtVersionOne()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));

        Assert.True(result.Succeeded);
        Assert.Equal("scene-1", result.SceneId);
        Assert.Equal(SceneLifecycle.Active, result.Lifecycle);
        Assert.Equal(1, harness.GetVersion("scene-1"));
    }

    [Fact]
    public async Task CreateSceneRejectsEmptySceneId()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CreateSceneAsync(
            new CreateSceneRequest(string.Empty, "session-1", "Forest Ambush", Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateSceneRejectsEmptySessionId()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", string.Empty, "Forest Ambush", Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateSceneRejectsEmptySceneName()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", string.Empty, Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseSceneTransitionsToClosed()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        var create = await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));
        Assert.True(create.Succeeded);

        var result = await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later));

        Assert.True(result.Succeeded);
        Assert.Equal(SceneLifecycle.Closed, result.Lifecycle);

        var reloadedState = harness.GetState("scene-1");
        Assert.Equal(SceneLifecycle.Closed, reloadedState.Lifecycle);
        Assert.Equal(Later, reloadedState.ClosedAt);
    }

    [Fact]
    public async Task CloseSceneFailsForUnknownScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("missing-scene", Later));

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseSceneFailsWhenAlreadyClosed()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));
        await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later));

        var second = await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later.AddDays(1)));

        Assert.False(second.Succeeded);
        Assert.Contains("closed", second.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSceneAsyncReturnsSceneForExistingScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));

        var result = await orchestrator.GetSceneAsync("scene-1");

        Assert.True(result.Found);
        Assert.Equal("scene-1", result.SceneId);
        Assert.Equal("session-1", result.SessionId);
        Assert.Equal("Forest Ambush", result.SceneName);
        Assert.Equal(SceneLifecycle.Active, result.Lifecycle);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task GetSceneAsyncReturnsClosedScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));
        await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later));

        var result = await orchestrator.GetSceneAsync("scene-1");

        Assert.True(result.Found);
        Assert.Equal(SceneLifecycle.Closed, result.Lifecycle);
        Assert.Equal(Later, result.ClosedAt);
    }

    [Fact]
    public async Task GetSceneAsyncReturnsNotFoundForUnknownScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.GetSceneAsync("never-created");

        Assert.False(result.Found);
        Assert.Null(result.SceneId);
    }

    [Fact]
    public async Task GetSceneAsyncIgnoresNonSceneDocuments()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        var unrelatedState = new SceneState(
            SceneId: "unrelated-scene",
            SessionId: "session-1",
            SceneName: "Unrelated",
            Lifecycle: SceneLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);
        var unrelatedDocument = new Document(
            Guid.NewGuid(),
            "some-other-content-type/v1",
            SceneSerializer.Serialize(unrelatedState),
            Version: 0);
        await harness.Store.SaveAsync(unrelatedDocument, expectedVersion: null);

        var result = await orchestrator.GetSceneAsync("unrelated-scene");

        Assert.False(result.Found);
    }

    [Fact]
    public async Task GetSceneAsyncPropagatesDeserializationFailureForMalformedSceneDocument()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        harness.InjectMalformedSceneDocument("malformed-scene", "this is not valid json");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() =>
            orchestrator.GetSceneAsync("any-scene"));
    }

    [Fact]
    public async Task StateSurvivesRehydrationThroughDocument()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));
        await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later));

        var state = harness.GetState("scene-1");
        Assert.Equal("scene-1", state.SceneId);
        Assert.Equal("session-1", state.SessionId);
        Assert.Equal("Forest Ambush", state.SceneName);
        Assert.Equal(SceneLifecycle.Closed, state.Lifecycle);
        Assert.Equal(Now, state.CreatedAt);
        Assert.Equal(Later, state.ClosedAt);
    }

    [Fact]
    public async Task DocumentVersionIncrementsAcrossOperations()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));
        await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("scene-1", Later));

        Assert.Equal(2, harness.GetVersion("scene-1"));
    }

    [Fact]
    public async Task FindBySceneIdAsyncReturnsGuidForExistingScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);
        await orchestrator.CreateSceneAsync(
            new CreateSceneRequest("scene-1", "session-1", "Forest Ambush", Now));

        var resolved = await orchestrator.FindBySceneIdAsync("scene-1");

        Assert.NotNull(resolved);
        Assert.Equal(harness.GetAggregateId("scene-1"), resolved.Value);
    }

    [Fact]
    public async Task FindBySceneIdAsyncReturnsNullForUnknownScene()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var resolved = await orchestrator.FindBySceneIdAsync("never-created");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task PersistenceFailureIsNotReportedAsSuccess()
    {
        var harness = new InMemorySceneHarness();
        var orchestrator = new SceneOrchestrator(harness.Store);

        var result = await orchestrator.CloseSceneAsync(
            new CloseSceneRequest("missing-scene", Later));

        Assert.False(result.Succeeded);
    }

    private sealed class InMemorySceneHarness
    {
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public AggregateStore Store { get; }

        public InMemorySceneHarness()
        {
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
        }

        public Guid GetAggregateId(string sceneId) => ids[sceneId];

        public long GetVersion(string sceneId) => documents[sceneId].Version;

        public SceneState GetState(string sceneId) =>
            SceneSerializer.Deserialize(documents[sceneId].PayloadJson);

        public void InjectMalformedSceneDocument(string sceneIdKey, string payloadJson)
        {
            var document = new Document(
                Guid.NewGuid(),
                SceneSerializer.ContentType,
                payloadJson,
                Version: 0);
            documents[sceneIdKey] = document;
            ids[sceneIdKey] = document.Id;
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
                var state = SceneSerializer.Deserialize(document.PayloadJson);
                if (documents.TryGetValue(state.SceneId, out var existing))
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
                    documents[state.SceneId] = updated;
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
                    documents[state.SceneId] = created;
                    ids[state.SceneId] = created.Id;
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, created, null));
                }
            }

            public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                foreach (var (sceneId, document) in documents.ToArray())
                {
                    if (document.Id == id)
                    {
                        documents.Remove(sceneId);
                        ids.Remove(sceneId);
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