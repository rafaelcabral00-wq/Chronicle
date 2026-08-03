using System.Text.Json;
using Chronicle.RuleSets.Abstractions.Manifests;

namespace Chronicle.RuleSets.Abstractions.PackageSources;

public sealed record RuleSetPackageSourceValidationResult(
    bool IsValid,
    IReadOnlyList<RuleSetPackageSourceFinding> Findings,
    IReadOnlyList<string> FileInventory);

public sealed record RuleSetPackageSourceFinding(
    RuleSetPackageSourceFindingSeverity Severity,
    RuleSetPackageSourceErrorCode Code,
    string Path,
    string Message);

public enum RuleSetPackageSourceFindingSeverity
{
    Error,
    Warning
}

public enum RuleSetPackageSourceErrorCode
{
    MissingManifest,
    MalformedManifestJson,
    InvalidManifestContract,
    MissingLocalizationResource,
    UndeclaredResource,
    PathTraversal,
    ProhibitedPrototypeEvidence,
    ProtectedSourceMaterial,
    ForbiddenSecret,
    ForbiddenGeneratedOutput,
    ForbiddenBinary,
    ForbiddenAbsolutePath,
    DisabledOperationNotEnforced,
    NonDeterministicInventory
}

public static class RuleSetPackageSourceValidator
{
    private const string ManifestRelativePath = "Metadata/werewolf.package-manifest.json";

    private static readonly string[] RequiredPackageSourceFiles =
    [
        "Chronicle.RuleSets.Werewolf.csproj",
        "CharacterCreation/WerewolfAttributePrioritySelection.cs",
        "CharacterCreation/WerewolfAttributeAllocation.cs",
        "CharacterCreation/WerewolfAuspiceSelection.cs",
        "CharacterCreation/WerewolfCharacterCreationDraftContracts.cs",
        "CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs",
        "CharacterCreation/WerewolfInitialGiftSelection.cs",
        "CharacterCreation/WerewolfMetisDeformitySelection.cs",
        "CharacterCreation/WerewolfRaceSelection.cs",
        "CharacterCreation/WerewolfTribeSelection.cs",
        "WerewolfRuleSetPackage.cs",
        "WerewolfReferenceRuntime.cs",
        "Metadata/current-slice.json",
        ManifestRelativePath
    ];

    private static readonly string[] ProhibitedEvidenceTokens =
    [
        "reviews/",
        "review-record",
        "review-evidence",
        "review-ledger",
        "catalog-review",
        "prototype-readiness",
        "prototype-work-status"
    ];

    private static readonly string[] ProtectedSourceTokens =
    [
        "sourcebook-text",
        "sourcebook text",
        "\"rawSourceIncluded\": true",
        "\"sourcebookTextIncluded\": true"
    ];

    private static readonly string[] SecretTokens =
    [
        "api_key=",
        "apikey=",
        "password=",
        "secret=",
        "private_key=",
        "BEGIN PRIVATE KEY"
    ];

    private static readonly string[] AbsolutePathPrefixes =
    [
        "C:\\",
        "D:\\",
        "/home/",
        "/Users/",
        "\\\\"
    ];

    public static RuleSetPackageSourceValidationResult Validate(string packageSourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageSourcePath);

        var root = Path.GetFullPath(packageSourcePath);
        var findings = new List<RuleSetPackageSourceFinding>();

        if (!Directory.Exists(root))
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.MissingManifest, ManifestRelativePath, "Package source root does not exist."));
            return BuildResult(findings, []);
        }

        var inventory = BuildInventory(root, findings);
        ValidateRequiredFiles(inventory, findings);
        ValidateInventoryOrdering(inventory, findings);
        ValidateFilePolicy(root, inventory, findings);
        ValidateManifest(root, findings);

        return BuildResult(findings, inventory);
    }

    private static string[] BuildInventory(string root, List<RuleSetPackageSourceFinding> findings)
    {
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetFullPath(path))
            .Where(path => !IsUnderGeneratedOutput(root, path))
            .Select(path => ToRelativePath(root, path, findings))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToArray();

        return files.Order(StringComparer.Ordinal).ToArray();
    }

    private static void ValidateRequiredFiles(IReadOnlyCollection<string> inventory, List<RuleSetPackageSourceFinding> findings)
    {
        foreach (var requiredFile in RequiredPackageSourceFiles)
        {
            if (!inventory.Contains(requiredFile, StringComparer.Ordinal))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.MissingManifest, requiredFile, $"Required package source file '{requiredFile}' is missing."));
            }
        }
    }

    private static void ValidateInventoryOrdering(IReadOnlyList<string> inventory, List<RuleSetPackageSourceFinding> findings)
    {
        if (!inventory.SequenceEqual(inventory.Order(StringComparer.Ordinal), StringComparer.Ordinal))
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.NonDeterministicInventory, ".", "Package source file inventory is not deterministically ordered."));
        }
    }

    private static void ValidateFilePolicy(string root, IReadOnlyList<string> inventory, List<RuleSetPackageSourceFinding> findings)
    {
        var manifestDeclaredResources = GetDeclaredResources(root, findings);

        foreach (var relativePath in inventory)
        {
            var normalized = relativePath.Replace('\\', '/');
            var fullPath = Path.Combine(root, relativePath);

            if (IsBinaryFile(relativePath))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ForbiddenBinary, relativePath, "Binary files are not allowed in package source."));
                continue;
            }

            if (normalized.Contains("/bin/", StringComparison.Ordinal) ||
                normalized.Contains("/obj/", StringComparison.Ordinal) ||
                normalized.EndsWith(".g.cs", StringComparison.Ordinal))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ForbiddenGeneratedOutput, relativePath, "Generated build output is not allowed in package source."));
            }

            if (ProhibitedEvidenceTokens.Any(token => normalized.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ProhibitedPrototypeEvidence, relativePath, "Prototype review or evidence files are not package source."));
            }

            var text = File.ReadAllText(fullPath);
            if (ProtectedSourceTokens.Any(token => text.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ProtectedSourceMaterial, relativePath, "Protected source material or raw source inclusion is prohibited."));
            }

            if (SecretTokens.Any(token => text.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ForbiddenSecret, relativePath, "Potential secret material is prohibited in package source."));
            }

            if (AbsolutePathPrefixes.Any(prefix => text.Contains(prefix, StringComparison.Ordinal)))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.ForbiddenAbsolutePath, relativePath, "Local absolute paths are prohibited in package source."));
            }

            if (!IsDeclaredResource(relativePath, manifestDeclaredResources))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.UndeclaredResource, relativePath, "Package source file is not declared by the manifest contract allow-list."));
            }
        }
    }

    private static void ValidateManifest(string root, List<RuleSetPackageSourceFinding> findings)
    {
        var manifestPath = Path.Combine(root, ManifestRelativePath);
        if (!File.Exists(manifestPath))
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.MissingManifest, ManifestRelativePath, "Package manifest is missing."));
            return;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        }
        catch (JsonException exception)
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.MalformedManifestJson, ManifestRelativePath, exception.Message));
            return;
        }

        using (document)
        {
            var manifestResult = RuleSetManifestValidator.Validate(document.RootElement);
            foreach (var error in manifestResult.Errors)
            {
                findings.Add(new RuleSetPackageSourceFinding(
                    RuleSetPackageSourceFindingSeverity.Error,
                    RuleSetPackageSourceErrorCode.InvalidManifestContract,
                    $"manifest:{error.Path}",
                    error.Message));
            }

            ValidateLocalizationFiles(root, document.RootElement, findings);
            ValidateDisabledOperationDeclarations(document.RootElement, findings);
        }
    }

    private static void ValidateLocalizationFiles(string root, JsonElement manifest, List<RuleSetPackageSourceFinding> findings)
    {
        foreach (var locale in manifest.GetProperty("supportedLocales").EnumerateArray())
        {
            var localeName = locale.GetString() ?? string.Empty;
            var relativePath = $"Localization/{localeName}/current-slice.json";
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

            if (!IsInsideRoot(root, fullPath) || relativePath.Contains("..", StringComparison.Ordinal))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.PathTraversal, relativePath, "Localization path escapes the package root."));
                continue;
            }

            if (!File.Exists(fullPath))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.MissingLocalizationResource, relativePath, "Declared localization resource is missing."));
            }
        }
    }

    private static void ValidateDisabledOperationDeclarations(JsonElement manifest, List<RuleSetPackageSourceFinding> findings)
    {
        foreach (var operation in manifest.GetProperty("disabledOperations").EnumerateArray())
        {
            var status = operation.GetProperty("status").GetString();
            if (!StringComparer.Ordinal.Equals(status, "disabled"))
            {
                findings.Add(Error(RuleSetPackageSourceErrorCode.DisabledOperationNotEnforced, "disabledOperations", "Declared disabled operation must remain disabled."));
            }
        }
    }

    private static HashSet<string> GetDeclaredResources(string root, List<RuleSetPackageSourceFinding> findings)
    {
        var declared = new HashSet<string>(StringComparer.Ordinal)
        {
            "Chronicle.RuleSets.Werewolf.csproj",
            "CharacterCreation/WerewolfAttributePrioritySelection.cs",
            "CharacterCreation/WerewolfAttributeAllocation.cs",
            "CharacterCreation/WerewolfAuspiceSelection.cs",
            "CharacterCreation/WerewolfCharacterCreationDraftContracts.cs",
            "CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs",
            "CharacterCreation/WerewolfInitialGiftSelection.cs",
            "CharacterCreation/WerewolfMetisDeformitySelection.cs",
            "CharacterCreation/WerewolfRaceSelection.cs",
            "CharacterCreation/WerewolfTribeSelection.cs",
            "WerewolfRuleSetPackage.cs",
            "WerewolfReferenceRuntime.cs",
            "Metadata/current-slice.json",
            ManifestRelativePath
        };

        var manifestPath = Path.Combine(root, ManifestRelativePath);
        if (!File.Exists(manifestPath))
        {
            return declared;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            foreach (var locale in document.RootElement.GetProperty("supportedLocales").EnumerateArray())
            {
                declared.Add($"Localization/{locale.GetString()}/current-slice.json");
            }
        }
        catch (JsonException)
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.MalformedManifestJson, ManifestRelativePath, "Package manifest could not be parsed for resource declarations."));
        }

        return declared;
    }

    private static bool IsDeclaredResource(string relativePath, HashSet<string> declaredResources)
    {
        return declaredResources.Contains(relativePath.Replace('\\', '/'));
    }

    private static string? ToRelativePath(string root, string fullPath, List<RuleSetPackageSourceFinding> findings)
    {
        if (!IsInsideRoot(root, fullPath))
        {
            findings.Add(Error(RuleSetPackageSourceErrorCode.PathTraversal, fullPath, "Resolved file path escapes package root."));
            return null;
        }

        return Path.GetRelativePath(root, fullPath).Replace('\\', '/');
    }

    private static bool IsInsideRoot(string root, string fullPath)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(fullPath);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderGeneratedOutput(string root, string fullPath)
    {
        var relative = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
        return relative.StartsWith("bin/", StringComparison.Ordinal) ||
            relative.StartsWith("obj/", StringComparison.Ordinal);
    }

    private static bool IsBinaryFile(string relativePath)
    {
        var extension = Path.GetExtension(relativePath);
        return StringComparer.OrdinalIgnoreCase.Equals(extension, ".dll") ||
            StringComparer.OrdinalIgnoreCase.Equals(extension, ".pdb") ||
            StringComparer.OrdinalIgnoreCase.Equals(extension, ".exe") ||
            StringComparer.OrdinalIgnoreCase.Equals(extension, ".zip");
    }

    private static RuleSetPackageSourceValidationResult BuildResult(
        IEnumerable<RuleSetPackageSourceFinding> findings,
        IReadOnlyList<string> inventory)
    {
        var orderedFindings = findings
            .OrderBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Code.ToString(), StringComparer.Ordinal)
            .ThenBy(finding => finding.Message, StringComparer.Ordinal)
            .ToArray();

        return new RuleSetPackageSourceValidationResult(
            orderedFindings.All(finding => finding.Severity != RuleSetPackageSourceFindingSeverity.Error),
            orderedFindings,
            inventory.Order(StringComparer.Ordinal).ToArray());
    }

    private static RuleSetPackageSourceFinding Error(
        RuleSetPackageSourceErrorCode code,
        string path,
        string message)
    {
        return new RuleSetPackageSourceFinding(
            RuleSetPackageSourceFindingSeverity.Error,
            code,
            path.Replace('\\', '/'),
            message);
    }
}
