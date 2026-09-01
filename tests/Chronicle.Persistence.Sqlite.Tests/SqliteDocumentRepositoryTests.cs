using Chronicle.Application.Persistence;
using Xunit;

namespace Chronicle.Persistence.Sqlite.Tests;

public sealed class SqliteDocumentRepositoryTests
{
    private static string CreateIsolatedDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), "chronicle-e1-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return $"Data Source={Path.Combine(directory, "e1.db")}";
    }

    [Fact]
    public async Task SaveAsyncCreatesDocumentAtVersionOne()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var document = new Document(id, "test-type", "{\"hello\":\"world\"}", 0);

        var result = await repository.SaveAsync(document, expectedVersion: null);

        Assert.Equal(DocumentPersistenceStatus.Succeeded, result.Status);
        Assert.NotNull(result.Document);
        Assert.Equal(1, result.Document!.Version);
        Assert.Equal(id, result.Document.Id);
        Assert.Equal("test-type", result.Document.ContentType);
        Assert.Equal("{\"hello\":\"world\"}", result.Document.PayloadJson);
    }

    [Fact]
    public async Task LoadAsyncReturnsPreviouslyPersistedDocument()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var document = new Document(id, "test-type", "{\"hello\":\"world\"}", 0);
        await repository.SaveAsync(document, expectedVersion: null);

        var loaded = await repository.LoadAsync(id);

        Assert.Equal(DocumentPersistenceStatus.Succeeded, loaded.Status);
        Assert.NotNull(loaded.Document);
        Assert.Equal(id, loaded.Document!.Id);
        Assert.Equal("test-type", loaded.Document.ContentType);
        Assert.Equal("{\"hello\":\"world\"}", loaded.Document.PayloadJson);
        Assert.Equal(1, loaded.Document.Version);
    }

    [Fact]
    public async Task LoadAsyncReturnsNotFoundForUnknownDocument()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();

        var result = await repository.LoadAsync(Guid.NewGuid());

        Assert.Equal(DocumentPersistenceStatus.NotFound, result.Status);
        Assert.Null(result.Document);
    }

    [Fact]
    public async Task SaveAsyncUpdatesExistingDocumentAtNextVersion()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var first = new Document(id, "test-type", "{\"v\":1}", 0);
        var created = await repository.SaveAsync(first, expectedVersion: null);
        Assert.Equal(1, created.Document!.Version);

        var second = new Document(id, "test-type", "{\"v\":2}", created.Document.Version);
        var updated = await repository.SaveAsync(second, expectedVersion: created.Document.Version);

        Assert.Equal(DocumentPersistenceStatus.Succeeded, updated.Status);
        Assert.Equal(2, updated.Document!.Version);
    }

    [Fact]
    public async Task SaveAsyncRejectsUpdateWithStaleExpectedVersion()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var first = new Document(id, "test-type", "{\"v\":1}", 0);
        await repository.SaveAsync(first, expectedVersion: null);

        var stale = new Document(id, "test-type", "{\"v\":2}", 999);
        var conflict = await repository.SaveAsync(stale, expectedVersion: 999);

        Assert.Equal(DocumentPersistenceStatus.ConcurrencyConflict, conflict.Status);
    }

    [Fact]
    public async Task SaveAsyncRejectsCreateWhenDocumentAlreadyExists()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var first = new Document(id, "test-type", "{\"v\":1}", 0);
        await repository.SaveAsync(first, expectedVersion: null);

        var duplicate = new Document(id, "test-type", "{\"v\":1}", 0);
        var conflict = await repository.SaveAsync(duplicate, expectedVersion: null);

        Assert.Equal(DocumentPersistenceStatus.ConcurrencyConflict, conflict.Status);
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingDocument()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var id = Guid.NewGuid();
        var document = new Document(id, "test-type", "{\"v\":1}", 0);
        await repository.SaveAsync(document, expectedVersion: null);

        var delete = await repository.DeleteAsync(id);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, delete.Status);

        var load = await repository.LoadAsync(id);
        Assert.Equal(DocumentPersistenceStatus.NotFound, load.Status);
    }

    [Fact]
    public async Task DeleteAsyncReturnsNotFoundForUnknownDocument()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();

        var result = await repository.DeleteAsync(Guid.NewGuid());

        Assert.Equal(DocumentPersistenceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task IsolatedDatabasesDoNotInterfere()
    {
        using var harnessA = new SqliteRepositoryHarness();
        using var harnessB = new SqliteRepositoryHarness();
        var repositoryA = harnessA.CreateRepository();
        var repositoryB = harnessB.CreateRepository();
        var id = Guid.NewGuid();
        await repositoryA.SaveAsync(new Document(id, "A", "{\"a\":1}", 0), expectedVersion: null);

        var loadOnB = await repositoryB.LoadAsync(id);
        Assert.Equal(DocumentPersistenceStatus.NotFound, loadOnB.Status);
    }

    [Fact]
    public async Task SchemaInitializationIsIdempotentAcrossConnections()
    {
        using var harness = new SqliteRepositoryHarness();
        var repository = harness.CreateRepository();
        var first = await repository.SaveAsync(new Document(Guid.NewGuid(), "t", "{}", 0), expectedVersion: null);
        var second = await repository.SaveAsync(new Document(Guid.NewGuid(), "t", "{}", 0), expectedVersion: null);

        Assert.Equal(DocumentPersistenceStatus.Succeeded, first.Status);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, second.Status);
    }

    [Fact]
    public void ConstructorRejectsEmptyConnectionString()
    {
        Assert.Throws<ArgumentException>(() => new SqliteDocumentRepository(string.Empty));
        Assert.Throws<ArgumentException>(() => new SqliteDocumentRepository("   "));
    }

    private sealed class SqliteRepositoryHarness : IDisposable
    {
        private readonly string directory;

        public SqliteRepositoryHarness()
        {
            directory = Path.Combine(Path.GetTempPath(), "chronicle-e1-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        public SqliteDocumentRepository CreateRepository()
        {
            return new SqliteDocumentRepository($"Data Source={Path.Combine(directory, "e1.db")}");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // best-effort cleanup; test isolation relies on unique temp directory
            }
        }
    }
}
