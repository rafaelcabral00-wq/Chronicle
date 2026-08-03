using Xunit;

namespace Chronicle.Tools.PackageValidator.Tests;

public sealed class PackageValidatorCommandTests
{
    [Fact]
    public void ValidWerewolfPackageReturnsValidExitCode()
    {
        var result = Run(WerewolfPackagePath());

        Assert.Equal((int)PackageValidatorExitCode.ValidPackage, result.ExitCode);
        Assert.Contains("Status: valid", result.Output, StringComparison.Ordinal);
        Assert.Contains("Findings: 0", result.Output, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void InvalidPackageReturnsValidationFailureExitCode()
    {
        using var package = PackageSourceCopy.Create();
        File.WriteAllText(Path.Combine(package.Root, "unexpected.txt"), "undeclared resource");

        var result = Run(package.Root);

        Assert.Equal((int)PackageValidatorExitCode.ValidationFailure, result.ExitCode);
        Assert.Contains("Status: invalid", result.Output, StringComparison.Ordinal);
        Assert.Contains("UndeclaredResource", result.Output, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void MissingPathReturnsInvalidInvocationExitCode()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = PackageValidatorCommand.Run([], output, error);

        Assert.Equal((int)PackageValidatorExitCode.InvalidInvocation, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Usage:", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void PackagePathOutsideRepositoryIsValidatedWhenExplicitlySupplied()
    {
        using var package = PackageSourceCopy.Create();

        var result = Run(package.Root);

        Assert.Equal((int)PackageValidatorExitCode.ValidPackage, result.ExitCode);
        Assert.Contains("Status: valid", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void OutputIsDeterministicForRepeatedValidation()
    {
        var first = Run(WerewolfPackagePath());
        var second = Run(WerewolfPackagePath());

        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.Output, second.Output);
        Assert.Equal(first.Error, second.Error);
    }

    [Fact]
    public void ExitCodesAreStable()
    {
        Assert.Equal(0, (int)PackageValidatorExitCode.ValidPackage);
        Assert.Equal(1, (int)PackageValidatorExitCode.ValidationFailure);
        Assert.Equal(2, (int)PackageValidatorExitCode.InvalidInvocation);
        Assert.Equal(3, (int)PackageValidatorExitCode.UnexpectedInternalFailure);
    }

    [Fact]
    public void ValidationDoesNotMutateFilesystem()
    {
        using var package = PackageSourceCopy.Create();
        var before = Snapshot(package.Root);

        _ = Run(package.Root);

        Assert.Equal(before, Snapshot(package.Root));
    }

    [Fact]
    public void ToolProjectDoesNotReferenceForbiddenRuntimeDependencies()
    {
        var projectFile = Path.Combine(FindRepositoryRoot(), "tools", "Chronicle.Tools.PackageValidator", "Chronicle.Tools.PackageValidator.csproj");
        var projectText = File.ReadAllText(projectFile);
        var forbidden = new[]
        {
            "Chronicle.Persistence",
            "Chronicle.Presentation",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Microsoft.EntityFrameworkCore",
            "OpenAI"
        };

        Assert.DoesNotContain(forbidden, token => projectText.Contains(token, StringComparison.Ordinal));
    }

    private static CommandResult Run(string packageSourcePath)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = PackageValidatorCommand.Run([packageSourcePath], output, error);
        return new CommandResult(exitCode, output.ToString(), error.ToString());
    }

    private static string WerewolfPackagePath()
    {
        return Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf");
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

    private static string[] Snapshot(string root)
    {
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private sealed record CommandResult(int ExitCode, string Output, string Error);

    private sealed class PackageSourceCopy : IDisposable
    {
        private PackageSourceCopy(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static PackageSourceCopy Create()
        {
            var source = WerewolfPackagePath();
            var target = Path.Combine(Path.GetTempPath(), $"chronicle-package-validator-{Guid.NewGuid():N}");
            CopyDirectory(source, target);
            return new PackageSourceCopy(target);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            {
                if (directory.Contains($"{Path.DirectorySeparatorChar}bin", StringComparison.Ordinal) ||
                    directory.Contains($"{Path.DirectorySeparatorChar}obj", StringComparison.Ordinal))
                {
                    continue;
                }

                Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
            }

            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                    file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                {
                    continue;
                }

                var destination = Path.Combine(target, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new InvalidOperationException("Destination file has no directory."));
                File.Copy(file, destination);
            }
        }
    }
}
