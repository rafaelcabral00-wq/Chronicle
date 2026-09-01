using System.Xml.Linq;
using Xunit;

namespace Chronicle.Architecture.Tests;

/// <summary>
/// E1 architecture guarantees: persistence leaks no further than
/// <c>Chronicle.Application</c>; <c>Chronicle.Domain</c> remains free of
/// every other Chronicle production project; <c>Chronicle.Persistence.Sqlite</c>
/// continues to depend in the correct direction. None of the pre-existing
/// assertions in <see cref="ProjectDependencyRulesTests"/> are weakened
/// by this file; it adds new invariants only.
/// </summary>
public sealed class E1LayeringRulesTests
{
    [Fact]
    public void DomainDoesNotDependOnApplicationInfrastructureOrPersistence()
    {
        var graph = LoadProjectReferenceGraph();
        var domain = graph["Chronicle.Domain"];
        var forbidden = new[]
        {
            "Chronicle.Application",
            "Chronicle.Infrastructure",
            "Chronicle.Persistence.Sqlite",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Chronicle.Presentation.Desktop",
            "Chronicle.Desktop"
        };

        Assert.Empty(domain.Intersect(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void ApplicationDoesNotDependOnConcreteSqliteImplementation()
    {
        var graph = LoadProjectReferenceGraph();
        var application = graph["Chronicle.Application"];

        Assert.DoesNotContain("Chronicle.Persistence.Sqlite", application);
        Assert.DoesNotContain("Chronicle.Infrastructure", application);
        Assert.DoesNotContain("Chronicle.Presentation.Desktop", application);
        Assert.DoesNotContain("Chronicle.Desktop", application);
    }

    [Fact]
    public void PersistenceSqliteMayDependOnApplicationAndDomainOnly()
    {
        var graph = LoadProjectReferenceGraph();
        var persistence = graph["Chronicle.Persistence.Sqlite"];
        var allowed = new[]
        {
            "Chronicle.Application",
            "Chronicle.Domain",
            "Chronicle.Contracts",
            "Chronicle.RuleSets.Abstractions"
        };

        Assert.Empty(persistence.Except(allowed, StringComparer.Ordinal));
    }

    [Fact]
    public void WerewolfIsNotReachableFromCoreProjects()
    {
        var graph = LoadProjectReferenceGraph();
        var coreProjects = new[]
        {
            "Chronicle.Domain",
            "Chronicle.Application",
            "Chronicle.Infrastructure",
            "Chronicle.Persistence.Sqlite",
            "Chronicle.Presentation.Desktop"
        };

        foreach (var project in coreProjects)
        {
            Assert.DoesNotContain("Chronicle.RuleSets.Werewolf", graph[project]);
        }
    }

    [Fact]
    public void ApplicationDependsOnDomain()
    {
        var graph = LoadProjectReferenceGraph();
        var application = graph["Chronicle.Application"];

        Assert.Contains("Chronicle.Domain", application);
    }

    private static Dictionary<string, string[]> LoadProjectReferenceGraph()
    {
        var root = FindRepositoryRoot();
        var projects = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        return projects.ToDictionary(
            path => Path.GetFileNameWithoutExtension(path),
            path => LoadProjectReferences(path),
            StringComparer.Ordinal);
    }

    private static string[] LoadProjectReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var projectDirectory = Path.GetDirectoryName(projectPath) ?? throw new InvalidOperationException("Project path has no directory.");

        return document.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(include!, projectDirectory))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(projectName => projectName is not null)
            .Select(projectName => projectName!)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }
}
