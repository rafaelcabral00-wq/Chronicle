using System.Text.Json.Nodes;
using Chronicle.RuleSets.Abstractions.Manifests;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class RuleSetManifestValidatorTests
{
    [Fact]
    public void WerewolfManifestIsValid()
    {
        var result = RuleSetManifestValidator.ValidateJson(LoadManifestJson());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MissingRequiredFieldsAreRejected()
    {
        var manifest = LoadManifestNode();
        manifest.Remove("packageId");

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.MissingRequiredField);
    }

    [Fact]
    public void MalformedSemanticVersionsAreRejected()
    {
        var manifest = LoadManifestNode();
        manifest["packageVersion"] = "version-one";

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.MalformedSemanticVersion);
    }

    [Fact]
    public void UnsupportedContractVersionsAreRejected()
    {
        var manifest = LoadManifestNode();
        manifest["contractVersion"] = 99;

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.UnsupportedContractVersion);
    }

    [Fact]
    public void DuplicateCapabilitiesAreRejected()
    {
        var manifest = LoadManifestNode();
        var capabilities = manifest["capabilities"]!.AsArray();
        capabilities.Add(JsonNode.Parse(capabilities[0]!.ToJsonString()));

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.DuplicateCapability);
    }

    [Fact]
    public void ContradictorySupportedAndDisabledCapabilitiesAreRejected()
    {
        var manifest = LoadManifestNode();
        var disabledOperations = manifest["disabledOperations"]!.AsArray();
        var firstDisabledOperation = disabledOperations[0]!.AsObject();
        firstDisabledOperation["capabilityKey"] = "character-creation";

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.ContradictoryCapabilityDeclaration);
    }

    [Fact]
    public void InvalidLocalizationDeclarationsAreRejected()
    {
        var manifest = LoadManifestNode();
        manifest["canonicalLanguage"] = "fr";

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.InvalidLocalizationDeclaration);
    }

    [Fact]
    public void InvalidCompatibilityRangesAreRejected()
    {
        var manifest = LoadManifestNode();
        var compatibility = manifest["compatibility"]!.AsObject();
        compatibility["maximumChronicleVersionPolicy"] = "any-version";

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.InvalidCompatibilityRange);
    }

    [Fact]
    public void NonDeterministicOrderingIsRejected()
    {
        var manifest = LoadManifestNode();
        manifest["supportedLocales"] = new JsonArray("pt-BR", "en");

        var result = Validate(manifest);

        Assert.Contains(result.Errors, error => error.Code == RuleSetManifestValidationErrorCode.NonDeterministicOrdering);
    }

    [Fact]
    public void ValidationIsDeterministicAndDoesNotMutateFilesystem()
    {
        var manifestJson = LoadManifestJson();
        var tempRoot = Directory.CreateTempSubdirectory("chronicle-manifest-validation-");

        try
        {
            var first = RuleSetManifestValidator.ValidateJson(manifestJson);
            var second = RuleSetManifestValidator.ValidateJson(manifestJson);
            var createdFiles = Directory.EnumerateFileSystemEntries(tempRoot.FullName).ToArray();

            Assert.Equal(first, second);
            Assert.Empty(createdFiles);
        }
        finally
        {
            tempRoot.Delete(true);
        }
    }

    private static RuleSetManifestValidationResult Validate(JsonObject manifest)
    {
        return RuleSetManifestValidator.ValidateJson(manifest.ToJsonString());
    }

    private static JsonObject LoadManifestNode()
    {
        return JsonNode.Parse(LoadManifestJson())!.AsObject();
    }

    private static string LoadManifestJson()
    {
        return File.ReadAllText(PackageFile("Metadata", "werewolf.package-manifest.json"));
    }

    private static string PackageFile(params string[] pathParts)
    {
        return Path.Combine([PackageRoot(), .. pathParts]);
    }

    private static string PackageRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "rule-sets", "Chronicle.RuleSets.Werewolf");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find Werewolf package source root.");
    }
}
