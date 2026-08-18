using System.Collections.ObjectModel;
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
        AssertRenownForAuspice(auspice, result.Draft.Renown);
        Assert.DoesNotContain(result.Findings, finding => finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Warning);
        Assert.DoesNotContain(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, result.Draft.RequiredNextSteps);
    }

    [Fact]
    public void RagabashReceivesNoInitialRenownButRequiresSelection()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.DoesNotContain(result.Findings, finding => finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Warning);
        Assert.DoesNotContain(WerewolfResourceRankInitializationService.InitializeResourcesAndRankStep, result.Draft!.RequiredNextSteps);
        Assert.Contains(WerewolfRagabashRenownSelectionService.SelectRagabashRenownStep, result.Draft.RequiredNextSteps);
        AssertRenownForAuspice(WerewolfAuspiceIdentifiers.Ragabash, result.Draft.Renown);
    }

    [Fact]
    public void OperationDoesNotMutatePreExistingRenownStructuralState()
    {
        var draft = Draft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Philodox);
        var originalRenown = draft.Renown;

        var result = Initialize(draft);

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.NotSame(originalRenown, result.Draft!.Renown);
        AssertRenownForAuspice(WerewolfAuspiceIdentifiers.Philodox, result.Draft.Renown);
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
        Assert.NotSame(first.Renown, second.Draft.Renown);
        AssertRenownForAuspice(WerewolfAuspiceIdentifiers.Theurge, second.Draft.Renown);
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
        Assert.Contains("character.renown.honor.permanent:3", initialized.Outputs["renown"], StringComparison.Ordinal);
        Assert.Contains("character.renown.honor.current:3", initialized.Outputs["renown"], StringComparison.Ordinal);
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

    private static void AssertRenownForAuspice(string auspice, IReadOnlyDictionary<string, int?> renown)
    {
        switch (auspice)
        {
            case WerewolfAuspiceIdentifiers.Ragabash:
                Assert.Empty(renown);
                break;
            case WerewolfAuspiceIdentifiers.Theurge:
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.GloryPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.GloryCurrent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.HonorPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.HonorCurrent]);
                Assert.Equal(3, renown[WerewolfRenownIdentifiers.WisdomPermanent]);
                Assert.Equal(3, renown[WerewolfRenownIdentifiers.WisdomCurrent]);
                break;
            case WerewolfAuspiceIdentifiers.Philodox:
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.GloryPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.GloryCurrent]);
                Assert.Equal(3, renown[WerewolfRenownIdentifiers.HonorPermanent]);
                Assert.Equal(3, renown[WerewolfRenownIdentifiers.HonorCurrent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.WisdomPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.WisdomCurrent]);
                break;
            case WerewolfAuspiceIdentifiers.Galliard:
                Assert.Equal(2, renown[WerewolfRenownIdentifiers.GloryPermanent]);
                Assert.Equal(2, renown[WerewolfRenownIdentifiers.GloryCurrent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.HonorPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.HonorCurrent]);
                Assert.Equal(1, renown[WerewolfRenownIdentifiers.WisdomPermanent]);
                Assert.Equal(1, renown[WerewolfRenownIdentifiers.WisdomCurrent]);
                break;
            case WerewolfAuspiceIdentifiers.Ahroun:
                Assert.Equal(2, renown[WerewolfRenownIdentifiers.GloryPermanent]);
                Assert.Equal(2, renown[WerewolfRenownIdentifiers.GloryCurrent]);
                Assert.Equal(1, renown[WerewolfRenownIdentifiers.HonorPermanent]);
                Assert.Equal(1, renown[WerewolfRenownIdentifiers.HonorCurrent]);
                 Assert.Equal(0, renown[WerewolfRenownIdentifiers.WisdomPermanent]);
                Assert.Equal(0, renown[WerewolfRenownIdentifiers.WisdomCurrent]);
                break;
        }
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

public sealed class WerewolfRagabashRenownSelectionTests
{
    private static WerewolfInitializedCharacterState BuildRagabashDraft()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("ragabash-draft-001"), 1) with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = null,
            RaceGift = WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            AttributePriorityOrder = Array.AsReadOnly(["physical", "social", "mental"]),
            AttributeBudgets = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["physical"] = 7,
                ["social"] = 5,
                ["mental"] = 3
            }),
            AbilityPriorityOrder = Array.AsReadOnly(["talents", "skills", "knowledges"]),
            AbilityBudgets = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["talents"] = 13,
                ["skills"] = 9,
                ["knowledges"] = 5
            }),
            Attributes = new ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                ["character.attribute.strength"] = 2,
                ["character.attribute.dexterity"] = 2,
                ["character.attribute.stamina"] = 2,
                ["character.attribute.charisma"] = 2,
                ["character.attribute.manipulation"] = 4,
                ["character.attribute.appearance"] = 2,
                ["character.attribute.perception"] = 3,
                ["character.attribute.intelligence"] = 3,
                ["character.attribute.wits"] = 2
            }),
            Abilities = new ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                ["character.ability.alertness"] = 2,
                ["character.ability.athletics"] = 2,
                ["character.ability.brawl"] = 2,
                ["character.ability.dodge"] = 0,
                ["character.ability.empathy"] = 2,
                ["character.ability.expression"] = 3,
                ["character.ability.intimidation"] = 1,
                ["character.ability.primal-instinct"] = 0,
                ["character.ability.streetwise"] = 0,
                ["character.ability.subterfuge"] = 3,
                ["character.ability.animal-empathy"] = 0,
                ["character.ability.crafts"] = 0,
                ["character.ability.drive"] = 2,
                ["character.ability.etiquette"] = 2,
                ["character.ability.firearms"] = 0,
                ["character.ability.leadership"] = 2,
                ["character.ability.melee"] = 0,
                ["character.ability.performance"] = 2,
                ["character.ability.stealth"] = 1,
                ["character.ability.survival"] = 0,
                ["character.ability.computer"] = 1,
                ["character.ability.enigmas"] = 0,
                ["character.ability.investigation"] = 1,
                ["character.ability.law"] = 1,
                ["character.ability.linguistics"] = 0,
                ["character.ability.medicine"] = 0,
                ["character.ability.occult"] = 1,
                ["character.ability.politics"] = 1,
                ["character.ability.rituals"] = 0,
                ["character.ability.science"] = 0
            }),
            Backgrounds = new ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                ["character.background.allies"] = 0,
                ["character.background.contacts"] = 0,
                ["character.background.mentor"] = 0,
                ["character.background.resources"] = 0,
                ["character.background.rites"] = 0
            }),
            Gifts = Array.AsReadOnly([
                WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
                WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
                WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine
            ])
        };

        var initialized = WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, draft.DraftVersion));
        return initialized.Draft!;
    }

    [Fact]
    public void RagabashInitializationLeavesSelectionPending()
    {
        var draft = BuildRagabashDraft();

        Assert.Empty(draft.Renown);
        Assert.Contains(WerewolfRagabashRenownSelectionService.SelectRagabashRenownStep, draft.RequiredNextSteps);
    }

    [Fact]
    public void ValidAllocation030IsAccepted()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 0, 0, 3));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Draft!.Renown[WerewolfRenownIdentifiers.GloryPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.HonorPermanent]);
        Assert.Equal(3, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.GloryCurrent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.HonorCurrent]);
        Assert.Equal(3, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomCurrent]);
    }

    [Fact]
    public void ValidAllocation300IsAccepted()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 3, 0, 0));

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Draft!.Renown[WerewolfRenownIdentifiers.GloryPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.HonorPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomPermanent]);
    }

    [Fact]
    public void ValidAllocation003IsAccepted()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 0, 3, 0));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Draft!.Renown[WerewolfRenownIdentifiers.GloryPermanent]);
        Assert.Equal(3, result.Draft.Renown[WerewolfRenownIdentifiers.HonorPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomPermanent]);
    }

    [Fact]
    public void ValidAllocation111IsAccepted()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 1, 1, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Draft!.Renown[WerewolfRenownIdentifiers.GloryPermanent]);
        Assert.Equal(1, result.Draft.Renown[WerewolfRenownIdentifiers.HonorPermanent]);
        Assert.Equal(1, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomPermanent]);
    }

    [Fact]
    public void ValidAllocation210IsAccepted()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 2, 1, 0));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Draft!.Renown[WerewolfRenownIdentifiers.GloryPermanent]);
        Assert.Equal(1, result.Draft.Renown[WerewolfRenownIdentifiers.HonorPermanent]);
        Assert.Equal(0, result.Draft.Renown[WerewolfRenownIdentifiers.WisdomPermanent]);
    }

    [Fact]
    public void TotalLessThanThreeIsRejected()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 1, 1, 0));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfRagabashRenownSelectionErrorCode.TotalMustBeThree);
    }

    [Fact]
    public void TotalGreaterThanThreeIsRejected()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 2, 1, 1));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfRagabashRenownSelectionErrorCode.TotalMustBeThree);
    }

    [Fact]
    public void NegativeAllocationIsRejected()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, -1, 2, 2));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfRagabashRenownSelectionErrorCode.NegativeAllocation);
    }

    [Fact]
    public void NonRagabashInvocationIsRejected()
    {
        var draft = BuildRagabashDraft();
        var theurgeDraft = draft with { Auspice = WerewolfAuspiceIdentifiers.Theurge };
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(theurgeDraft, theurgeDraft.DraftVersion, 0, 0, 3));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfRagabashRenownSelectionErrorCode.NotRagabash);
    }

    [Fact]
    public void StaleVersionIsRejected()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion - 1, 0, 0, 3));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfRagabashRenownSelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void SourceDraftRemainsImmutableOnFailure()
    {
        var draft = BuildRagabashDraft();
        var originalVersion = draft.DraftVersion;

        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion - 1, 0, 0, 3));

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
        Assert.Equal(originalVersion, draft.DraftVersion);
    }

    [Fact]
    public void VersionIncrementsExactlyOnceOnSuccess()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 1, 1, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(draft.DraftVersion + 1, result.Draft!.DraftVersion);
    }

    [Fact]
    public void RequiredNextStepsRemovedOnSuccess()
    {
        var draft = BuildRagabashDraft();
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 0, 0, 3));

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(WerewolfRagabashRenownSelectionService.SelectRagabashRenownStep, result.Draft!.RequiredNextSteps);
    }

    [Fact]
    public void CompletionBlockedBeforeSelection()
    {
        var draft = BuildRagabashDraft();
        var completion = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(completion.Succeeded);
        Assert.Contains(completion.Findings, f => f.Code == WerewolfCharacterCompletionErrorCode.RagabashRenownNotSelected);
    }

    [Fact]
    public void CompletionSucceedsAfterSelection()
    {
        var draft = BuildRagabashDraft();
        var selected = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 0, 0, 3));
        Assert.True(selected.Succeeded);

        var ready = selected.Draft! with { IdentityName = "test", RequiredNextSteps = Array.AsReadOnly<string>([]) };
        var completion = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(ready, ready.DraftVersion));

        Assert.True(completion.Succeeded, Format(completion.Findings));
    }

    [Fact]
    public void RuntimeRoundTripPreservesAllocation()
    {
        var draft = BuildRagabashDraft();
        var selected = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, draft.DraftVersion, 1, 1, 1));
        Assert.True(selected.Succeeded);

        var ready = selected.Draft! with { IdentityName = "test", RequiredNextSteps = Array.AsReadOnly<string>([]) };
        var completion = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(ready, ready.DraftVersion));
        Assert.True(completion.Succeeded, Format(completion.Findings));

        var state = WerewolfRuntimeCharacterState.FromSnapshot(completion.Snapshot!);
        Assert.Equal(1, state.GloryPermanent);
        Assert.Equal(1, state.HonorPermanent);
        Assert.Equal(1, state.WisdomPermanent);
        Assert.Equal(1, state.GloryCurrent);
        Assert.Equal(1, state.HonorCurrent);
        Assert.Equal(1, state.WisdomCurrent);
    }

    private static string Format(IEnumerable<WerewolfCharacterCompletionFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }
}
