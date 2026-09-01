using Chronicle.Application.Persistence;
using Xunit;

namespace Chronicle.Persistence.Sqlite.Tests;

public sealed class GenericDocumentSqliteRoundTripTests
{
    [Fact]
    public async Task DocumentRoundTripsThroughSqlite()
    {
        using var harness = new SqliteHarness();
        var repository = harness.CreateRepository();

        var document = new Document(
            Guid.NewGuid(),
            ContentType: "custom-aggregate",
            PayloadJson: "{\"hello\":\"world\"}",
            Version: 0);

        var save = await repository.SaveAsync(document, expectedVersion: null);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, save.Status);
        Assert.Equal(1, save.Document!.Version);

        var loaded = await repository.LoadAsync(document.Id);
        Assert.Equal(loaded.Document!.PayloadJson, document.PayloadJson);
        Assert.Equal(loaded.Document.ContentType, document.ContentType);
    }

    private sealed class SqliteHarness : IDisposable
    {
        private readonly string directory;

        public SqliteHarness()
        {
            directory = Path.Combine(Path.GetTempPath(), "chronicle-e2-doc-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        public SqliteDocumentRepository CreateRepository()
        {
            return new SqliteDocumentRepository($"Data Source={Path.Combine(directory, "doc.db")}");
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
            }
        }
    }
}
