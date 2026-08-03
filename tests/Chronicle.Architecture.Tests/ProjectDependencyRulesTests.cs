using System.Xml.Linq;
using Xunit;

namespace Chronicle.Architecture.Tests;

public sealed class ProjectDependencyRulesTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedProductionReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Chronicle.Domain"] = [],
            ["Chronicle.Contracts"] = [],
            ["Chronicle.RuleSets.Abstractions"] = ["Chronicle.Contracts", "Chronicle.Domain"],
            ["Chronicle.NarrativeIntelligence.Abstractions"] = ["Chronicle.Contracts"],
            ["Chronicle.Application"] =
            [
                "Chronicle.Domain",
                "Chronicle.Contracts",
                "Chronicle.RuleSets.Abstractions",
                "Chronicle.NarrativeIntelligence.Abstractions"
            ],
            ["Chronicle.Infrastructure"] =
            [
                "Chronicle.Application",
                "Chronicle.Contracts",
                "Chronicle.RuleSets.Abstractions",
                "Chronicle.NarrativeIntelligence.Abstractions"
            ],
            ["Chronicle.Persistence.Sqlite"] =
            [
                "Chronicle.Application",
                "Chronicle.Domain",
                "Chronicle.Contracts",
                "Chronicle.RuleSets.Abstractions"
            ],
            ["Chronicle.NarrativeIntelligence.OpenAI"] =
            [
                "Chronicle.NarrativeIntelligence.Abstractions",
                "Chronicle.Contracts",
                "Chronicle.Application"
            ],
            ["Chronicle.Presentation.Desktop"] =
            [
                "Chronicle.Application",
                "Chronicle.Contracts"
            ],
            ["Chronicle.Desktop"] =
            [
                "Chronicle.Application",
                "Chronicle.Contracts",
                "Chronicle.Infrastructure",
                "Chronicle.Persistence.Sqlite",
                "Chronicle.NarrativeIntelligence.OpenAI",
                "Chronicle.Presentation.Desktop"
            ]
        };

    private static readonly IReadOnlyDictionary<string, string[]> AllowedTestReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Chronicle.Domain.Tests"] = ["Chronicle.Domain"],
            ["Chronicle.Application.Tests"] = ["Chronicle.Application"],
            ["Chronicle.Architecture.Tests"] = AllowedProductionReferences.Keys.ToArray(),
            ["Chronicle.Contracts.Tests"] = ["Chronicle.Contracts"],
            ["Chronicle.Infrastructure.Tests"] = ["Chronicle.Infrastructure"],
            ["Chronicle.Persistence.Sqlite.Tests"] = ["Chronicle.Persistence.Sqlite"]
        };

    [Fact]
    public void ProductionProjectsReferenceOnlyAuthorizedProjects()
    {
        var graph = LoadProjectReferenceGraph();
        var violations = new List<string>();

        foreach (var (project, allowedReferences) in AllowedProductionReferences)
        {
            var actualReferences = graph.GetValueOrDefault(project, []);
            var disallowed = actualReferences.Except(allowedReferences, StringComparer.Ordinal);
            violations.AddRange(disallowed.Select(reference => $"{project} -> {reference}"));
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void DomainDependsOnNoChronicleProductionProject()
    {
        var graph = LoadProjectReferenceGraph();

        Assert.Empty(graph["Chronicle.Domain"]);
    }

    [Fact]
    public void ContractsDependsOnNoConcreteInfrastructureProject()
    {
        var graph = LoadProjectReferenceGraph();

        Assert.DoesNotContain("Chronicle.Infrastructure", graph["Chronicle.Contracts"]);
        Assert.DoesNotContain("Chronicle.Persistence.Sqlite", graph["Chronicle.Contracts"]);
        Assert.DoesNotContain("Chronicle.NarrativeIntelligence.OpenAI", graph["Chronicle.Contracts"]);
        Assert.DoesNotContain("Chronicle.Presentation.Desktop", graph["Chronicle.Contracts"]);
        Assert.DoesNotContain("Chronicle.Desktop", graph["Chronicle.Contracts"]);
    }

    [Fact]
    public void ApplicationDoesNotDependOnConcreteImplementationsOrPresentation()
    {
        var graph = LoadProjectReferenceGraph();
        var forbidden = new[]
        {
            "Chronicle.Infrastructure",
            "Chronicle.Persistence.Sqlite",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Chronicle.Presentation.Desktop",
            "Chronicle.Desktop"
        };

        Assert.Empty(graph["Chronicle.Application"].Intersect(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void AbstractionsDoNotDependOnConcreteImplementations()
    {
        var graph = LoadProjectReferenceGraph();
        var forbidden = new[]
        {
            "Chronicle.Infrastructure",
            "Chronicle.Persistence.Sqlite",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Chronicle.Presentation.Desktop",
            "Chronicle.Desktop"
        };

        Assert.Empty(graph["Chronicle.RuleSets.Abstractions"].Intersect(forbidden, StringComparer.Ordinal));
        Assert.Empty(graph["Chronicle.NarrativeIntelligence.Abstractions"].Intersect(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void InfrastructureAndPersistenceDoNotDependOnPresentationOrDesktop()
    {
        var graph = LoadProjectReferenceGraph();
        var forbidden = new[] { "Chronicle.Presentation.Desktop", "Chronicle.Desktop" };

        Assert.Empty(graph["Chronicle.Infrastructure"].Intersect(forbidden, StringComparer.Ordinal));
        Assert.Empty(graph["Chronicle.Persistence.Sqlite"].Intersect(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void PresentationDoesNotOwnPersistenceOrProviderImplementations()
    {
        var graph = LoadProjectReferenceGraph();
        var forbidden = new[]
        {
            "Chronicle.Infrastructure",
            "Chronicle.Persistence.Sqlite",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Chronicle.Desktop"
        };

        Assert.Empty(graph["Chronicle.Presentation.Desktop"].Intersect(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void DesktopIsTheOnlyCompositionRoot()
    {
        var graph = LoadProjectReferenceGraph();
        var desktopReferences = graph["Chronicle.Desktop"];

        Assert.Equal(
            AllowedProductionReferences["Chronicle.Desktop"].Order(StringComparer.Ordinal),
            desktopReferences.Order(StringComparer.Ordinal));

        foreach (var project in AllowedProductionReferences.Keys.Where(project => project != "Chronicle.Desktop"))
        {
            Assert.DoesNotContain("Chronicle.Desktop", graph[project]);
        }
    }

    [Fact]
    public void ProjectReferencesAreAcyclic()
    {
        var graph = LoadProjectReferenceGraph()
            .Where(entry => AllowedProductionReferences.ContainsKey(entry.Key))
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        var cycles = FindCycles(graph);

        Assert.Empty(cycles);
    }

    [Fact]
    public void TestProjectsReferenceOnlyAuthorizedProductionProjects()
    {
        var graph = LoadProjectReferenceGraph();
        var violations = new List<string>();

        foreach (var (project, allowedReferences) in AllowedTestReferences)
        {
            var actualReferences = graph.GetValueOrDefault(project, []);
            var disallowed = actualReferences.Except(allowedReferences, StringComparer.Ordinal);
            violations.AddRange(disallowed.Select(reference => $"{project} -> {reference}"));
        }

        Assert.Empty(violations);
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

    private static string[] FindCycles(IReadOnlyDictionary<string, string[]> graph)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new Stack<string>();
        var cycles = new List<string>();

        foreach (var project in graph.Keys)
        {
            Visit(project);
        }

        return cycles.ToArray();

        void Visit(string project)
        {
            if (visiting.Contains(project, StringComparer.Ordinal))
            {
                cycles.Add(string.Join(" -> ", visiting.Reverse().Append(project)));
                return;
            }

            if (!visited.Add(project))
            {
                return;
            }

            visiting.Push(project);

            foreach (var reference in graph.GetValueOrDefault(project, []))
            {
                if (graph.ContainsKey(reference))
                {
                    Visit(reference);
                }
            }

            visiting.Pop();
        }
    }
}
