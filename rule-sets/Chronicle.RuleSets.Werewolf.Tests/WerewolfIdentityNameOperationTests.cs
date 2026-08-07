using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfIdentityNameOperationTests
{
    [Fact]
    public void SuccessfulNameAssignment()
    {
        var draft = Draft();
        var result = SetName(draft, "Alice");

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal("Alice", result.Draft?.IdentityName);
        Assert.DoesNotContain(WerewolfIdentityNameOperation.SetIdentityNameStep, result.Draft!.RequiredNextSteps);
    }

    [Fact]
    public void TrimsWhitespace()
    {
        var draft = Draft();
        var result = SetName(draft, "  Alice  ");

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal("Alice", result.Draft?.IdentityName);
    }

    [Fact]
    public void AcceptsMinimumLength()
    {
        var draft = Draft();
        var result = SetName(draft, "A");

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal("A", result.Draft?.IdentityName);
    }

    [Fact]
    public void RejectsAboveMaximumLength()
    {
        var draft = Draft();
        var result = SetName(draft, new string('a', 121));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.IdentityNameTooLong);
        Assert.Null(result.Draft?.IdentityName);
    }

    [Fact]
    public void RejectsEmptyInput()
    {
        var draft = Draft();
        var result = SetName(draft, "");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.MissingIdentityName);
        Assert.Null(result.Draft?.IdentityName);
    }

    [Fact]
    public void RejectsWhitespaceOnlyInput()
    {
        var draft = Draft();
        var result = SetName(draft, "   ");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.IdentityNameWhitespace);
        Assert.Null(result.Draft?.IdentityName);
    }

    [Fact]
    public void RejectsMissingDraft()
    {
        var result = WerewolfIdentityNameOperation.SetIdentityName(new WerewolfIdentityNameRequest(null!, 1, "Alice"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.MissingDraft);
        Assert.Null(result.Draft);
    }

    [Fact]
    public void RejectsUninitializedDraft()
    {
        var draft = Draft() with { Status = (WerewolfCharacterDraftStatus)999 };

        var result = SetName(draft, "Alice");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.DraftNotInitialized);
        Assert.Null(result.Draft?.IdentityName);
    }

    [Fact]
    public void RejectsStaleVersion()
    {
        var draft = Draft();

        var result = WerewolfIdentityNameOperation.SetIdentityName(new WerewolfIdentityNameRequest(draft, draft.DraftVersion - 1, "Alice"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfIdentityNameErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void IncrementsVersionExactlyOnce()
    {
        var draft = Draft();
        var first = SetName(draft, "Alice").Draft!;
        var second = SetName(first with { DraftVersion = first.DraftVersion }, "Bob").Draft!;

        Assert.Equal(3, second.DraftVersion);
    }

    [Fact]
    public void PreservesUnrelatedState()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Philodox,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfBackgroundIdentifiers.Allies] = 2 },
            Resources = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfCharacterResourceIdentifiers.GnosisPermanent] = 1 }
        };

        var result = SetName(draft, "Alice");

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(WerewolfRaceIdentifiers.Metis, result.Draft?.Race);
        Assert.Equal(WerewolfAuspiceIdentifiers.Philodox, result.Draft?.Auspice);
        Assert.Equal(2, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Allies]);
        Assert.Equal(1, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.GnosisPermanent]);
    }

    [Fact]
    public void AllowsReassignment()
    {
        var draft = SetName(Draft(), "Alice").Draft!;

        var result = SetName(draft, "Bob");

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal("Bob", result.Draft?.IdentityName);
    }

    [Fact]
    public void DeterministicFindingsAndNextSteps()
    {
        var draft = Draft();
        var first = SetName(draft, "Alice");
        var second = SetName(draft, "Alice");

        Assert.Equal(first.Findings.Count, second.Findings.Count);
        Assert.Equal(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
    }

    [Fact]
    public void IdentityNameRequiredValidationRule()
    {
        Assert.False(WerewolfIdentityNameOperation.ValidateIdentityNameRequired(null).IsValid);
        Assert.False(WerewolfIdentityNameOperation.ValidateIdentityNameRequired("").IsValid);
        Assert.False(WerewolfIdentityNameOperation.ValidateIdentityNameRequired("   ").IsValid);
        Assert.True(WerewolfIdentityNameOperation.ValidateIdentityNameRequired("A").IsValid);
        Assert.True(WerewolfIdentityNameOperation.ValidateIdentityNameRequired("Alice").IsValid);
        Assert.Equal("character.completion.identity.name-required", WerewolfIdentityNameOperation.ValidateIdentityNameRequired(null).RuleKey);
    }

    [Fact]
    public void RuntimeRegistryInvokesSetIdentityName()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, Inputs(created.Outputs, ("raceId", WerewolfRaceIdentifiers.Homid))));
        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, Inputs(race.Outputs, ("auspiceId", WerewolfAuspiceIdentifiers.Philodox))));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, Inputs(auspice.Outputs, ("tribeId", WerewolfTribeIdentifiers.GlassWalkers))));
        var resources = registry.Execute(Request(WerewolfReferenceRuntime.InitializeResourcesAndRankOperation, Inputs(tribe.Outputs)));

        var named = registry.Execute(Request(WerewolfReferenceRuntime.SetIdentityNameOperation, Inputs(resources.Outputs, ("identityName", "Alice"))));

        Assert.True(named.Succeeded, Format(named.Findings));
        Assert.Equal("Alice", named.Outputs["identityName"]);
    }

    [Fact]
    public void PackageSourceValidationIncludesIdentityNameOperation()
    {
        var root = Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf");
        var validation = RuleSetPackageSourceValidator.Validate(root);

        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Findings.Select(f => $"{f.Severity}|{f.Code}|{f.Path}|{f.Message}")));
        Assert.Contains("CharacterCreation/WerewolfIdentityNameOperation.cs", validation.FileInventory);
    }

    [Fact]
    public void HasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfIdentityNameOperation.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void DeterministicRepeatedExecution()
    {
        var draft = Draft();
        var first = SetName(draft, "Alice");
        var second = SetName(draft, "Alice");

        Assert.Equal(first.Draft?.IdentityName, second.Draft?.IdentityName);
        Assert.Equal(first.Draft?.DraftVersion, second.Draft?.DraftVersion);
        Assert.Equal(first.Findings.Count, second.Findings.Count);
    }

    private static WerewolfIdentityNameResult SetName(WerewolfInitializedCharacterState draft, string name)
    {
        return WerewolfIdentityNameOperation.SetIdentityName(new WerewolfIdentityNameRequest(draft, draft.DraftVersion, name));
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1) with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Philodox,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers
        };
    }

    private static Dictionary<string, string> Inputs(IReadOnlyDictionary<string, string> outputs, params (string Key, string Value)[] additional)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = outputs["draftId"],
            ["draftVersion"] = outputs["draftVersion"],
            ["expectedDraftVersion"] = outputs["draftVersion"]
        };

        foreach (var pair in additional)
        {
            inputs[pair.Key] = pair.Value;
        }

        foreach (var key in new[] { "raceId", "auspiceId", "tribeId", "metisDeformityId", "raceGiftId", "auspiceGiftId", "tribeGiftId", "attributePriorityOrder", "attributeBudgets", "abilityPriorityOrder", "abilityBudgets", "attributes", "abilities", "backgrounds", "resources", "renown", "rankId", "rankValue", "identityName", "nextSteps", "status" })
        {
            if (outputs.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && !inputs.ContainsKey($"current{char.ToUpperInvariant(key[0])}{key[1..]}"))
            {
                inputs[key switch
                {
                    "raceId" => "currentRace",
                    "auspiceId" => "currentAuspice",
                    "tribeId" => "currentTribe",
                    "metisDeformityId" => "currentMetisDeformity",
                    "raceGiftId" => "currentRaceGift",
                    "auspiceGiftId" => "currentAuspiceGift",
                    "tribeGiftId" => "currentTribeGift",
                    "status" => "draftStatus",
                    _ => key
                }] = value;
            }
        }

        return inputs;
    }

    private static string Format(IEnumerable<WerewolfIdentityNameFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static string Format(IEnumerable<RuleSetRuntimeFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static RuleSetOperationRequest Request(string operationKey, IReadOnlyDictionary<string, string> inputs)
    {
        return new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            operationKey,
            inputs);
    }

    private static RuleSetRuntimeRegistry RuntimeRegistry()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([Path.Combine(FindRepositoryRoot(), "rule-sets")]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(registration.Catalog, [new WerewolfReferenceRuntime(new TestIdentitySource())])).Registry;
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

        throw new InvalidOperationException("Could not find repository root.");
    }

    private sealed class TestIdentitySource : IWerewolfCharacterDraftIdentitySource
    {
        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            return new WerewolfCharacterDraftIdentity("runtime-draft-001");
        }
    }
}
