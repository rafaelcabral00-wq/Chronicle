using Chronicle.Application.Session;
using Chronicle.Application.Persistence;
using Chronicle.Domain.Session;
using Xunit;

namespace Chronicle.Application.Tests.Session;

public sealed class SessionOrchestratorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateSessionPersistsAggregateAtVersionOne()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));

        Assert.True(result.Succeeded);
        Assert.Equal("session-1", result.SessionId);
        Assert.Equal(SessionLifecycle.Active, result.Lifecycle);
        Assert.Equal(1, harness.GetVersion("session-1"));
    }

    [Fact]
    public async Task CreateSessionRejectsEmptySessionId()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CreateSessionAsync(
            new CreateSessionRequest(string.Empty, "campaign-1", "First Session", Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateSessionRejectsEmptyCampaignId()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", string.Empty, "First Session", Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateSessionRejectsEmptySessionName()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", string.Empty, Now));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseSessionTransitionsToClosed()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        var create = await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));
        Assert.True(create.Succeeded);

        var result = await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later));

        Assert.True(result.Succeeded);
        Assert.Equal(SessionLifecycle.Closed, result.Lifecycle);

        var reloadedState = harness.GetState("session-1");
        Assert.Equal(SessionLifecycle.Closed, reloadedState.Lifecycle);
        Assert.Equal(Later, reloadedState.ClosedAt);
    }

    [Fact]
    public async Task CloseSessionFailsForUnknownSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("missing-session", Later));

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CloseSessionFailsWhenAlreadyClosed()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));
        await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later));

        var second = await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later.AddDays(1)));

        Assert.False(second.Succeeded);
        Assert.Contains("closed", second.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSessionAsyncReturnsSessionForExistingSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));

        var result = await orchestrator.GetSessionAsync("session-1");

        Assert.True(result.Found);
        Assert.Equal("session-1", result.SessionId);
        Assert.Equal("campaign-1", result.CampaignId);
        Assert.Equal("First Session", result.SessionName);
        Assert.Equal(SessionLifecycle.Active, result.Lifecycle);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task GetSessionAsyncReturnsClosedSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));
        await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later));

        var result = await orchestrator.GetSessionAsync("session-1");

        Assert.True(result.Found);
        Assert.Equal(SessionLifecycle.Closed, result.Lifecycle);
        Assert.Equal(Later, result.ClosedAt);
    }

    [Fact]
    public async Task GetSessionAsyncReturnsNotFoundForUnknownSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.GetSessionAsync("never-created");

        Assert.False(result.Found);
        Assert.Null(result.SessionId);
    }

    [Fact]
    public async Task GetSessionAsyncIgnoresNonSessionDocuments()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        var unrelatedState = new SessionState(
            SessionId: "unrelated-session",
            CampaignId: "campaign-1",
            SessionName: "Unrelated",
            Lifecycle: SessionLifecycle.Active,
            CreatedAt: Now,
            ClosedAt: null);
        var unrelatedDocument = new Document(
            Guid.NewGuid(),
            "some-other-content-type/v1",
            SessionSerializer.Serialize(unrelatedState),
            Version: 0);
        await harness.Store.SaveAsync(unrelatedDocument, expectedVersion: null);

        var result = await orchestrator.GetSessionAsync("unrelated-session");

        Assert.False(result.Found);
    }

    [Fact]
    public async Task GetSessionAsyncPropagatesDeserializationFailureForMalformedSessionDocument()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        harness.InjectMalformedSessionDocument("malformed-session", "this is not valid json");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() =>
            orchestrator.GetSessionAsync("any-session"));
    }

    [Fact]
    public async Task StateSurvivesRehydrationThroughDocument()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));
        await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later));

        var state = harness.GetState("session-1");
        Assert.Equal("session-1", state.SessionId);
        Assert.Equal("campaign-1", state.CampaignId);
        Assert.Equal("First Session", state.SessionName);
        Assert.Equal(SessionLifecycle.Closed, state.Lifecycle);
        Assert.Equal(Now, state.CreatedAt);
        Assert.Equal(Later, state.ClosedAt);
    }

    [Fact]
    public async Task DocumentVersionIncrementsAcrossOperations()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));
        await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("session-1", Later));

        Assert.Equal(2, harness.GetVersion("session-1"));
    }

    [Fact]
    public async Task FindBySessionIdAsyncReturnsGuidForExistingSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);
        await orchestrator.CreateSessionAsync(
            new CreateSessionRequest("session-1", "campaign-1", "First Session", Now));

        var resolved = await orchestrator.FindBySessionIdAsync("session-1");

        Assert.NotNull(resolved);
        Assert.Equal(harness.GetAggregateId("session-1"), resolved.Value);
    }

    [Fact]
    public async Task FindBySessionIdAsyncReturnsNullForUnknownSession()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var resolved = await orchestrator.FindBySessionIdAsync("never-created");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task PersistenceFailureIsNotReportedAsSuccess()
    {
        var harness = new InMemorySessionHarness();
        var orchestrator = new SessionOrchestrator(harness.Store);

        var result = await orchestrator.CloseSessionAsync(
            new CloseSessionRequest("missing-session", Later));

        Assert.False(result.Succeeded);
    }

    private sealed class InMemorySessionHarness
    {
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public AggregateStore Store { get; }

        public InMemorySessionHarness()
        {
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
        }

        public Guid GetAggregateId(string sessionId) => ids[sessionId];

        public long GetVersion(string sessionId) => documents[sessionId].Version;

        public SessionState GetState(string sessionId) =>
            SessionSerializer.Deserialize(documents[sessionId].PayloadJson);

        public void InjectMalformedSessionDocument(string sessionIdKey, string payloadJson)
        {
            var document = new Document(
                Guid.NewGuid(),
                SessionSerializer.ContentType,
                payloadJson,
                Version: 0);
            documents[sessionIdKey] = document;
            ids[sessionIdKey] = document.Id;
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
                var state = SessionSerializer.Deserialize(document.PayloadJson);
                if (documents.TryGetValue(state.SessionId, out var existing))
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
                    documents[state.SessionId] = updated;
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
                    documents[state.SessionId] = created;
                    ids[state.SessionId] = created.Id;
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, created, null));
                }
            }

            public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                foreach (var (sessionId, document) in documents.ToArray())
                {
                    if (document.Id == id)
                    {
                        documents.Remove(sessionId);
                        ids.Remove(sessionId);
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