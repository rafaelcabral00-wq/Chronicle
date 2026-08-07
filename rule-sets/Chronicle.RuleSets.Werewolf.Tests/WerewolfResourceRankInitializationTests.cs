using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfResourceRankInitializationTests
{
    public static TheoryData<string, int> RaceGnosis => new()
    {
        { WerewolfRaceIdentifiers.Homid, 1 },
        { WerewolfRaceIdentifiers.Metis, 3 },
        { WerewolfRaceIdentifiers.Lupus, 5 }
    };

    public static TheoryData<string, int> AuspiceRage => new()
    {
        { WerewolfAuspiceIdentifiers.Ragabash, 1 },
        { WerewolfAuspiceIdentifiers.Theurge, 2 },
        { WerewolfAuspiceIdentifiers.Philodox, 3 },
        { WerewolfAuspiceIdentifiers.Galliard, 4 },
        { WerewolfAuspiceIdentifiers.Ahroun, 5 }
    };

    [Theory]
    [MemberData(nameof(RaceGnosis))]
    public void DerivesGnosisForEverySupportedRace(string race, int expectedGnosis)
    {
        var draft = Draft(race, WerewolfAuspiceIdentifiers.Philodox);

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(expectedGnosis, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.GnosisPermanent]);
        Assert.Equal(expectedGnosis, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.GnosisCurrent]);
    }

    [Theory]
    [MemberData(nameof(AuspiceRage))]
    public void DerivesRageForEverySupportedAuspice(string auspice, int expectedRage)
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, auspice);

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(expectedRage, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.RagePermanent]);
        Assert.Equal(expectedRage, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.RageCurrent]);
        Assert.NotNull(result.Draft);
        Assert.All(result.Draft.Renown.Values, Assert.Null);
        Assert.DoesNotContain(result.Findings, finding => finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Warning);
        Assert.DoesNotContain(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, result.Draft.RequiredNextSteps);
    }

    [Fact]
    public void RagabashReceivesNoRenownFindingOrNextStep()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.DoesNotContain(result.Findings, finding => finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Warning);
        Assert.DoesNotContain(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, result.Draft!.RequiredNextSteps);
        Assert.All(result.Draft.Renown.Values, Assert.Null);
    }

    [Fact]
    public void OperationDoesNotMutatePreExistingRenownStructuralState()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Philodox);
        var originalRenown = draft.Renown;

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Same(originalRenown, result.Draft!.Renown);
        Assert.All(result.Draft.Renown.Values, Assert.Null);
    }

    [Fact]
    public void InitializesGlassWalkerWillpowerCliathRankAndKeepsResourcesBackgroundSeparate()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Ahroun) with
        {
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 2,
                [WerewolfBackgroundIdentifiers.Contacts] = 1,
                [WerewolfBackgroundIdentifiers.Mentor] = 0,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 1
            }
        };

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(3, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.WillpowerPermanent]);
        Assert.Equal(3, result.Draft?.Resources[WerewolfCharacterResourceIdentifiers.WillpowerCurrent]);
        Assert.Equal(WerewolfRankIdentifiers.Cliath, result.Draft?.Rank);
        Assert.Equal(1, result.Draft?.RankValue);
        Assert.Equal(1, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Resources]);
        Assert.False(result.Draft?.Resources.ContainsKey(WerewolfBackgroundIdentifiers.Resources));
        Assert.DoesNotContain(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RequiresPrerequisitesAndExactDraftVersion()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Philodox);

        AssertCode(draft with { Race = null }, WerewolfResourceRankInitializationErrorCode.MissingRace);
        AssertCode(draft with { Auspice = null }, WerewolfResourceRankInitializationErrorCode.MissingAuspice);
        AssertCode(draft with { Tribe = null }, WerewolfResourceRankInitializationErrorCode.MissingTribe);
        AssertCode(draft with { Race = WerewolfRaceIdentifiers.Metis, MetisDeformity = null }, WerewolfResourceRankInitializationErrorCode.MissingMetisDeformity);

        var stale = WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, draft.DraftVersion - 1));
        Assert.False(stale.Succeeded);
        Assert.Contains(stale.Findings, finding => finding.Code == WerewolfResourceRankInitializationErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReinitializesAtomicallyAndImmutably()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Theurge);
        var first = Initialize(draft).Draft!;
        var second = Initialize(first with { Race = WerewolfRaceIdentifiers.Lupus, DraftVersion = first.DraftVersion });

        Assert.True(second.Succeeded, Format(second.Findings));
        Assert.NotSame(first.Resources, second.Draft!.Resources);
        Assert.Same(first.Renown, second.Draft.Renown);
        Assert.All(second.Draft.Renown.Values, Assert.Null);
        Assert.Equal(5, second.Draft.Resources[WerewolfCharacterResourceIdentifiers.GnosisPermanent]);
        Assert.Equal(first.DraftVersion + 1, second.Draft.DraftVersion);
    }

    [Fact]
    public void PreservesPriorDraftStateAndDisabledCapabilities()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Galliard) with
        {
            RaceGift = WerewolfInitialGiftIdentifiers.MetisCreateElement,
            AuspiceGift = WerewolfInitialGiftIdentifiers.GalliardBeastSpeech,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            AttributePriorityOrder = Array.AsReadOnly([WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental]),
            AbilityPriorityOrder = Array.AsReadOnly([WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges]),
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Strength] = 5 },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Alertness] = 3 },
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfBackgroundIdentifiers.Resources] = 1 }
        };

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(draft.RaceGift, result.Draft?.RaceGift);
        Assert.Equal(draft.AuspiceGift, result.Draft?.AuspiceGift);
        Assert.Equal(draft.TribeGift, result.Draft?.TribeGift);
        Assert.Equal(draft.AttributePriorityOrder, result.Draft?.AttributePriorityOrder);
        Assert.Equal(draft.AbilityPriorityOrder, result.Draft?.AbilityPriorityOrder);
        Assert.Equal(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(draft.Abilities, result.Draft?.Abilities);
        Assert.Equal(draft.Backgrounds, result.Draft?.Backgrounds);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
    }

    [Fact]
    public void ClassificationChangeClearsStaleInitializedValues()
    {
        var initialized = Initialize(Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Philodox)).Draft!;

        var changed = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(initialized, initialized.DraftVersion, WerewolfRaceIdentifiers.Lupus));

        Assert.True(changed.Succeeded);
        Assert.All(changed.Draft!.Resources.Values, Assert.Null);
        Assert.All(changed.Draft!.Renown.Values, Assert.Null);
        Assert.Null(changed.Draft.Rank);
        Assert.Null(changed.Draft.RankValue);
        Assert.Contains(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, changed.Draft.RequiredNextSteps);
    }

    [Fact]
    public void RuntimeRegistryInvokesResourceRankInitialization()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, Inputs(created.Outputs, ("raceId", WerewolfRaceIdentifiers.Homid))));
        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, Inputs(race.Outputs, ("auspiceId", WerewolfAuspiceIdentifiers.Philodox))));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, Inputs(auspice.Outputs, ("tribeId", WerewolfTribeIdentifiers.GlassWalkers))));

        var initialized = registry.Execute(Request(WerewolfReferenceRuntime.InitializeResourcesAndRankOperation, Inputs(tribe.Outputs)));

        Assert.True(initialized.Succeeded, Format(initialized.Findings));
        Assert.Equal("5", initialized.Outputs["draftVersion"]);
        Assert.Equal(WerewolfRankIdentifiers.Cliath, initialized.Outputs["rankId"]);
        Assert.Equal("1", initialized.Outputs["rankValue"]);
        Assert.Contains("character.resource.rage.permanent:3", initialized.Outputs["resources"], StringComparison.Ordinal);
        Assert.Contains("character.resource.gnosis.current:1", initialized.Outputs["resources"], StringComparison.Ordinal);
        Assert.Contains("character.resource.willpower.permanent:3", initialized.Outputs["resources"], StringComparison.Ordinal);
        Assert.False(initialized.Outputs.TryGetValue("renown", out var renownOutput) && !string.IsNullOrEmpty(renownOutput));
    }

    [Fact]
    public void ResourceRankInitializationHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfResourceRankInitialization.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfResourceRankInitializationResult Initialize(WerewolfInitializedCharacterState draft)
    {
        return WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, draft.DraftVersion));
    }

    private static void AssertCode(WerewolfInitializedCharacterState draft, WerewolfResourceRankInitializationErrorCode code)
    {
        var result = Initialize(draft);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == code);
    }

    private static WerewolfInitializedCharacterState Draft(string race, string auspice)
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1) with
        {
            Race = race,
            Auspice = auspice,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = StringComparer.Ordinal.Equals(race, WerewolfRaceIdentifiers.Metis) ? WerewolfMetisDeformityIdentifiers.Horns : null
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

    private static string Format(IEnumerable<RuleSetRuntimeFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static string Format(IEnumerable<WerewolfResourceRankInitializationFinding> findings)
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
