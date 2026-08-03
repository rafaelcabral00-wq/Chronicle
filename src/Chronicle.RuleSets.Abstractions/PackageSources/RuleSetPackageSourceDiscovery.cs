using System.Text.Json;

namespace Chronicle.RuleSets.Abstractions.PackageSources;

public sealed record RuleSetPackageSourceDiscoveryRequest(IReadOnlyList<string> AuthorizedSearchRoots);

public sealed record RuleSetPackageSourceDiscoveryResult(
    IReadOnlyList<RuleSetPackageSourceDescriptor> ValidatedPackages,
    IReadOnlyList<RuleSetPackageSourceRejection> RejectedPackages);

public sealed record RuleSetPackageSourceDescriptor(
    string PackageSourcePath,
    string PackageId,
    string PackageIdStatus,
    string PackageVersion,
    string DeclaredScopeId,
    IReadOnlyList<string> Capabilities,
    RuleSetPackageSourceDiscoveryValidationStatus ValidationStatus,
    IReadOnlyList<RuleSetPackageSourceFinding> Findings);

public sealed record RuleSetPackageSourceRejection(
    string PackageSourcePath,
    RuleSetPackageSourceDiscoveryErrorCode Code,
    string Message,
    IReadOnlyList<RuleSetPackageSourceFinding> Findings);

public enum RuleSetPackageSourceDiscoveryValidationStatus
{
    Valid,
    Rejected
}

public enum RuleSetPackageSourceDiscoveryErrorCode
{
    SearchRootMissing,
    MalformedCandidate,
    ValidationFailed,
    DuplicatePackageIdentity
}

public static class RuleSetPackageSourceDiscoveryService
{
    private const string ManifestRelativePath = "Metadata/werewolf.package-manifest.json";

    public static RuleSetPackageSourceDiscoveryResult Discover(RuleSetPackageSourceDiscoveryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var descriptors = new List<RuleSetPackageSourceDescriptor>();
        var rejections = new List<RuleSetPackageSourceRejection>();

        foreach (var root in request.AuthorizedSearchRoots
                     .Where(root => !string.IsNullOrWhiteSpace(root))
                     .Select(Path.GetFullPath)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Order(StringComparer.Ordinal))
        {
            if (!Directory.Exists(root))
            {
                rejections.Add(new RuleSetPackageSourceRejection(
                    root,
                    RuleSetPackageSourceDiscoveryErrorCode.SearchRootMissing,
                    "Authorized search root does not exist.",
                    []));
                continue;
            }

            foreach (var candidate in Directory.EnumerateDirectories(root)
                         .Select(Path.GetFullPath)
                         .Order(StringComparer.Ordinal))
            {
                var manifestPath = Path.Combine(candidate, ManifestRelativePath);
                if (!File.Exists(manifestPath))
                {
                    continue;
                }

                InspectCandidate(candidate, manifestPath, descriptors, rejections);
            }
        }

        DetectDuplicates(descriptors, rejections);

        var rejectedPaths = rejections
            .Where(rejection => rejection.Code == RuleSetPackageSourceDiscoveryErrorCode.DuplicatePackageIdentity)
            .Select(rejection => rejection.PackageSourcePath)
            .ToHashSet(StringComparer.Ordinal);

        var accepted = descriptors
            .Where(descriptor => !rejectedPaths.Contains(descriptor.PackageSourcePath))
            .OrderBy(descriptor => descriptor.PackageId, StringComparer.Ordinal)
            .ThenBy(descriptor => descriptor.PackageVersion, StringComparer.Ordinal)
            .ThenBy(descriptor => descriptor.PackageSourcePath, StringComparer.Ordinal)
            .ToArray();

        return new RuleSetPackageSourceDiscoveryResult(
            accepted,
            rejections
                .OrderBy(rejection => rejection.PackageSourcePath, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.Code.ToString(), StringComparer.Ordinal)
                .ThenBy(rejection => rejection.Message, StringComparer.Ordinal)
                .ToArray());
    }

    private static void InspectCandidate(
        string candidate,
        string manifestPath,
        List<RuleSetPackageSourceDescriptor> descriptors,
        List<RuleSetPackageSourceRejection> rejections)
    {
        RuleSetPackageSourceValidationResult validationResult;
        try
        {
            validationResult = RuleSetPackageSourceValidator.Validate(candidate);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            rejections.Add(new RuleSetPackageSourceRejection(
                candidate,
                RuleSetPackageSourceDiscoveryErrorCode.MalformedCandidate,
                exception.Message,
                []));
            return;
        }

        ManifestIdentity identity;
        try
        {
            identity = ReadManifestIdentity(manifestPath);
        }
        catch (Exception exception) when (exception is JsonException or IOException or KeyNotFoundException or InvalidOperationException)
        {
            rejections.Add(new RuleSetPackageSourceRejection(
                candidate,
                RuleSetPackageSourceDiscoveryErrorCode.MalformedCandidate,
                exception.Message,
                validationResult.Findings));
            return;
        }

        var descriptor = new RuleSetPackageSourceDescriptor(
            candidate,
            identity.PackageId,
            identity.PackageIdStatus,
            identity.PackageVersion,
            identity.DeclaredScopeId,
            identity.Capabilities,
            validationResult.IsValid
                ? RuleSetPackageSourceDiscoveryValidationStatus.Valid
                : RuleSetPackageSourceDiscoveryValidationStatus.Rejected,
            validationResult.Findings);

        if (validationResult.IsValid)
        {
            descriptors.Add(descriptor);
            return;
        }

        rejections.Add(new RuleSetPackageSourceRejection(
            candidate,
            RuleSetPackageSourceDiscoveryErrorCode.ValidationFailed,
            "Candidate package source failed canonical validation.",
            validationResult.Findings));
    }

    private static void DetectDuplicates(
        IReadOnlyCollection<RuleSetPackageSourceDescriptor> descriptors,
        List<RuleSetPackageSourceRejection> rejections)
    {
        foreach (var duplicateGroup in descriptors
                     .GroupBy(descriptor => $"{descriptor.PackageId}@{descriptor.PackageVersion}", StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            foreach (var descriptor in duplicateGroup.OrderBy(descriptor => descriptor.PackageSourcePath, StringComparer.Ordinal))
            {
                rejections.Add(new RuleSetPackageSourceRejection(
                    descriptor.PackageSourcePath,
                    RuleSetPackageSourceDiscoveryErrorCode.DuplicatePackageIdentity,
                    $"Duplicate package identity '{descriptor.PackageId}' version '{descriptor.PackageVersion}'.",
                    descriptor.Findings));
            }
        }
    }

    private static ManifestIdentity ReadManifestIdentity(string manifestPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var manifest = document.RootElement;

        return new ManifestIdentity(
            manifest.GetProperty("packageId").GetString() ?? string.Empty,
            manifest.GetProperty("packageIdStatus").GetString() ?? string.Empty,
            manifest.GetProperty("packageVersion").GetString() ?? string.Empty,
            manifest.GetProperty("declaredReleaseScope").GetProperty("scopeId").GetString() ?? string.Empty,
            manifest.GetProperty("capabilities")
                .EnumerateArray()
                .Select(capability => capability.GetProperty("capabilityKey").GetString() ?? string.Empty)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private sealed record ManifestIdentity(
        string PackageId,
        string PackageIdStatus,
        string PackageVersion,
        string DeclaredScopeId,
        IReadOnlyList<string> Capabilities);
}
