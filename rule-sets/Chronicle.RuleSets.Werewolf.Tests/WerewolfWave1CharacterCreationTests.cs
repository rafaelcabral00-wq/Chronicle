using System.Collections.ObjectModel;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 1 correctness regression tests for character creation:
/// validation fingerprint coverage, Lupus ability restrictions, and
/// tribe eligibility blockers.
/// </summary>
public sealed class WerewolfWave1CharacterCreationTests
{
    // ---------------------------------------------------------------------
    // Item 2: the validation fingerprint must include allocated Attribute
    // values (source lines 1088-1183 define nine Attributes).
    // ---------------------------------------------------------------------
    [Fact]
    public void ValidationFingerprintChangesWhenAnAttributeValueChanges()
    {
        var baseline = BuildDraft();
        var stronger = BuildDraft() with
        {
            Attributes = new Dictionary<string, int?>(baseline.Attributes, StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 4
            }
        };

        var baselineFingerprint = Fingerprint(baseline);
        var strongerFingerprint = Fingerprint(stronger);

        Assert.NotEqual(baselineFingerprint, strongerFingerprint);
    }

    [Fact]
    public void ValidationFingerprintIsStableForIdenticalState()
    {
        var first = Fingerprint(BuildDraft());
        var second = Fingerprint(BuildDraft());

        Assert.Equal(first, second);
    }

    [Fact]
    public void ValidationFingerprintIsIndependentOfAttributeInsertionOrder()
    {
        var draft = BuildDraft();
        var reordered = draft with
        {
            Attributes = new Dictionary<string, int?>(
                draft.Attributes.OrderByDescending(entry => entry.Key, StringComparer.Ordinal)
                    .ToDictionary(entry => entry.Key, entry => entry.Value),
                StringComparer.Ordinal)
        };

        Assert.Equal(Fingerprint(draft), Fingerprint(reordered));
    }

    [Fact]
    public void ValidationFingerprintStillCoversAbilitiesAndBackgrounds()
    {
        var draft = BuildDraft();

        var abilityChanged = draft with
        {
            Abilities = new Dictionary<string, int?>(draft.Abilities, StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Brawl] = 3
            }
        };

        var backgroundChanged = draft with
        {
            Backgrounds = new Dictionary<string, int?>(draft.Backgrounds, StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 3
            }
        };

        var baseFingerprint = Fingerprint(draft);

        Assert.NotEqual(baseFingerprint, Fingerprint(abilityChanged));
        Assert.NotEqual(baseFingerprint, Fingerprint(backgroundChanged));
    }

    [Fact]
    public void ValidationFingerprintCoversEveryAllocatedAttributeKey()
    {
        // Proves the fingerprint iterates the nine real Attribute identifiers
        // rather than the physical/mental/social category identifiers.
        var baseline = Fingerprint(BuildDraft());

        foreach (var attributeId in WerewolfCharacterCreationDraftFactory.GetAllocatedAttributeKeys())
        {
            var draft = BuildDraft();
            var current = draft.Attributes.GetValueOrDefault(attributeId) ?? 0;
            var mutated = draft with
            {
                Attributes = new Dictionary<string, int?>(draft.Attributes, StringComparer.Ordinal)
                {
                    [attributeId] = current == 5 ? 1 : current + 1
                }
            };

            Assert.NotEqual(baseline, Fingerprint(mutated));
        }
    }

    // ---------------------------------------------------------------------
    // Item 3: Lupus base-ability restriction.
    // Source line 547 explicitly restricts nine Abilities from initial
    // points for Lupus: Crafts, Drive, Etiquette, Firearms, Computer, Law,
    // Linguistics, Politics, Science. The restriction is source-backed and
    // is scoped to base allocation only (bonus points may still buy them).
    // ---------------------------------------------------------------------
    public static IEnumerable<object[]> SourceRestrictedLupusAbilities()
    {
        yield return [WerewolfAbilityIdentifiers.Crafts];
        yield return [WerewolfAbilityIdentifiers.Drive];
        yield return [WerewolfAbilityIdentifiers.Etiquette];
        yield return [WerewolfAbilityIdentifiers.Firearms];
        yield return [WerewolfAbilityIdentifiers.Computer];
        yield return [WerewolfAbilityIdentifiers.Law];
        yield return [WerewolfAbilityIdentifiers.Linguistics];
        yield return [WerewolfAbilityIdentifiers.Politics];
        yield return [WerewolfAbilityIdentifiers.Science];
    }

    [Fact]
    public void LupusRestrictedAbilityListMatchesSourceLine547Exactly()
    {
        var expected = new[]
            {
                WerewolfAbilityIdentifiers.Crafts,
                WerewolfAbilityIdentifiers.Drive,
                WerewolfAbilityIdentifiers.Etiquette,
                WerewolfAbilityIdentifiers.Firearms,
                WerewolfAbilityIdentifiers.Computer,
                WerewolfAbilityIdentifiers.Law,
                WerewolfAbilityIdentifiers.Linguistics,
                WerewolfAbilityIdentifiers.Politics,
                WerewolfAbilityIdentifiers.Science
            }
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actual = WerewolfAbilitySelectionService.LupusBaseRestrictedAbilities.Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(expected, actual);
        Assert.Equal(9, actual.Length);
    }

    [Theory]
    [MemberData(nameof(SourceRestrictedLupusAbilities))]
    public void SourceRestrictedLupusAbilitiesAreRefusedAtBaseAllocation(string abilityId)
    {
        var result = WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(
            BuildLupusDraft(abilityId, 1),
            ExpectedDraftVersion: 1,
            Allocations: BuildLupusAllocations(abilityId, rating: 1)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding =>
            string.Equals(finding.Message, $"Lupus base allocation cannot assign dots to the restricted Ability '{abilityId}' (source line 547).", StringComparison.Ordinal));
    }

    [Fact]
    public void NonRestrictedLupusAbilitiesRemainAllocatableAtBase()
    {
        foreach (var abilityId in new[]
                 {
                     WerewolfAbilityIdentifiers.Athletics,
                     WerewolfAbilityIdentifiers.Alertness,
                     WerewolfAbilityIdentifiers.Melee,
                     WerewolfAbilityIdentifiers.Occult,
                     WerewolfAbilityIdentifiers.Stealth,
                     WerewolfAbilityIdentifiers.Survival
                 })
        {
            var result = WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(
                BuildLupusDraft(abilityId, 1),
                ExpectedDraftVersion: 1,
                Allocations: BuildLupusAllocations(abilityId, rating: 1)));

            Assert.True(result.Succeeded, $"Expected '{abilityId}' to be allocatable for Lupus but got: {string.Join("; ", result.Findings.Select(f => f.Message))}");
        }
    }

    // ---------------------------------------------------------------------
    // Item 4: Silver Fangs must be selectable before backgrounds are
    // allocated. Source line 779 requires Pure Breed >= 3, and the
    // completion gate re-validates with the final background values, so a
    // not-yet-allocated background must not permanently block the tribe.
    // ---------------------------------------------------------------------
    [Fact]
    public void SilverFangsAreNotBlockedWhenPureBreedIsNotYetAllocated()
    {
        var unallocated = new Dictionary<string, int?>(StringComparer.Ordinal);

        var findings = WerewolfTribeEligibilityService.CheckBackgroundMinimums(
            WerewolfTribeIdentifiers.SilverFangs,
            unallocated);

        Assert.Empty(findings);
    }

    [Fact]
    public void SilverFangsRemainBlockedAtCompletionWhenPureBreedIsBelowThree()
    {
        var belowMinimum = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfBackgroundIdentifiers.PureBreed] = 2
        };

        var findings = WerewolfTribeEligibilityService.CheckBackgroundMinimums(
            WerewolfTribeIdentifiers.SilverFangs,
            belowMinimum);

        var finding = Assert.Single(findings);
        Assert.Equal(WerewolfTribeEligibilityErrorCode.BackgroundMinimumNotMet, finding.Code);
    }

    [Fact]
    public void SilverFangsAreEligibleWithPureBreedThreeOrMore()
    {
        var atMinimum = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfBackgroundIdentifiers.PureBreed] = 3
        };

        Assert.Empty(WerewolfTribeEligibilityService.CheckBackgroundMinimums(
            WerewolfTribeIdentifiers.SilverFangs,
            atMinimum));
    }

    [Fact]
    public void BlackFuriesRemainBlockedBecauseNoGenderFieldExists()
    {
        // Source line 713 defines the tribe as exclusively female, but the
        // character draft has no gender field. The blocker is retained so the
        // rule is never silently violated; it is a content-parity gap.
        var findings = WerewolfTribeEligibilityService.CheckDependencies(
            WerewolfTribeIdentifiers.BlackFuries,
            new Dictionary<string, int?>(StringComparer.Ordinal));

        var finding = Assert.Single(findings);
        Assert.Equal(WerewolfTribeEligibilityErrorCode.DependencyUnavailable, finding.Code);
    }

    private static string Fingerprint(WerewolfInitializedCharacterState draft) =>
        WerewolfCharacterCompletionOperation.ComputeValidationFingerprint(draft);

    private static List<WerewolfAbilityDotAllocation> BuildLupusAllocations(string raisedAbilityId, int rating)
    {
        var allocations = new List<WerewolfAbilityDotAllocation>();
        foreach (var abilityId in WerewolfAbilityIdentifiers.Supported)
        {
            allocations.Add(new WerewolfAbilityDotAllocation(abilityId, StringComparer.Ordinal.Equals(abilityId, raisedAbilityId) ? rating : 0));
        }

        return allocations;
    }

    private static WerewolfInitializedCharacterState BuildLupusDraft(string raisedAbilityId, int rating)
    {
        // Budgets must exactly match the spend so the restriction is what is
        // under test rather than a budget mismatch.
        var budgets = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAbilityCategoryIdentifiers.Talents] = 0,
            [WerewolfAbilityCategoryIdentifiers.Skills] = 0,
            [WerewolfAbilityCategoryIdentifiers.Knowledges] = 0
        };

        budgets[AbilityCategoryOf(raisedAbilityId)] = rating;

        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-lupus"),
            1) with
        {
            Race = WerewolfRaceIdentifiers.Lupus,
            Auspice = WerewolfAuspiceIdentifiers.Galliard,
            AbilityPriorityOrder = Array.AsReadOnly(
            [
                WerewolfAbilityCategoryIdentifiers.Talents,
                WerewolfAbilityCategoryIdentifiers.Skills,
                WerewolfAbilityCategoryIdentifiers.Knowledges
            ]),
            AbilityBudgets = new ReadOnlyDictionary<string, int>(budgets)
        };
    }

    private static string AbilityCategoryOf(string abilityId)
    {
        return AbilityCategoriesById.GetValueOrDefault(abilityId, WerewolfAbilityCategoryIdentifiers.Knowledges);
    }

    private static readonly Dictionary<string, string> AbilityCategoriesById =
        new(WerewolfAbilitySelectionService.AbilityCategoryMap, StringComparer.Ordinal);

    private static WerewolfInitializedCharacterState BuildDraft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-fingerprint"),
            1) with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Galliard,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            RaceGift = WerewolfGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais,
            TribeGift = WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine,
            AttributePriorityOrder = Array.AsReadOnly([WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental]),
            AbilityPriorityOrder = Array.AsReadOnly([WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges]),
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 2,
                [WerewolfAttributeIdentifiers.Manipulation] = 2,
                [WerewolfAttributeIdentifiers.Appearance] = 2,
                [WerewolfAttributeIdentifiers.Perception] = 2,
                [WerewolfAttributeIdentifiers.Intelligence] = 2,
                [WerewolfAttributeIdentifiers.Wits] = 2
            },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Alertness] = 2,
                [WerewolfAbilityIdentifiers.Athletics] = 2,
                [WerewolfAbilityIdentifiers.Brawl] = 1,
                [WerewolfAbilityIdentifiers.Dodge] = 0,
                [WerewolfAbilityIdentifiers.Empathy] = 1,
                [WerewolfAbilityIdentifiers.Expression] = 1,
                [WerewolfAbilityIdentifiers.Intimidation] = 1,
                [WerewolfAbilityIdentifiers.PrimalInstinct] = 0,
                [WerewolfAbilityIdentifiers.Streetwise] = 0,
                [WerewolfAbilityIdentifiers.Subterfuge] = 1,
                [WerewolfAbilityIdentifiers.AnimalEmpathy] = 1,
                [WerewolfAbilityIdentifiers.Crafts] = 0,
                [WerewolfAbilityIdentifiers.Drive] = 0,
                [WerewolfAbilityIdentifiers.Etiquette] = 0,
                [WerewolfAbilityIdentifiers.Firearms] = 0,
                [WerewolfAbilityIdentifiers.Leadership] = 1,
                [WerewolfAbilityIdentifiers.Melee] = 1,
                [WerewolfAbilityIdentifiers.Performance] = 0,
                [WerewolfAbilityIdentifiers.Stealth] = 1,
                [WerewolfAbilityIdentifiers.Survival] = 1,
                [WerewolfAbilityIdentifiers.Computer] = 0,
                [WerewolfAbilityIdentifiers.Enigmas] = 0,
                [WerewolfAbilityIdentifiers.Investigation] = 0,
                [WerewolfAbilityIdentifiers.Law] = 0,
                [WerewolfAbilityIdentifiers.Linguistics] = 0,
                [WerewolfAbilityIdentifiers.Medicine] = 0,
                [WerewolfAbilityIdentifiers.Occult] = 0,
                [WerewolfAbilityIdentifiers.Politics] = 0,
                [WerewolfAbilityIdentifiers.Rituals] = 0,
                [WerewolfAbilityIdentifiers.Science] = 0
            },
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 2,
                [WerewolfBackgroundIdentifiers.Ancestors] = 1,
                [WerewolfBackgroundIdentifiers.Contacts] = 0,
                [WerewolfBackgroundIdentifiers.Fetish] = 0,
                [WerewolfBackgroundIdentifiers.Kinfolk] = 1,
                [WerewolfBackgroundIdentifiers.Mentor] = 0,
                [WerewolfBackgroundIdentifiers.PureBreed] = 0,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 0
            },
            Resources = BuildResources(),
            Renown = BuildRenown(),
            Rank = "Cliath",
            RankValue = 1,
            IdentityName = "Test Character"
        };
    }

    private static Dictionary<string, int?> BuildResources()
    {
        return new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfCharacterResourceIdentifiers.RagePermanent] = 5,
            [WerewolfCharacterResourceIdentifiers.RageCurrent] = 5,
            [WerewolfCharacterResourceIdentifiers.GnosisPermanent] = 1,
            [WerewolfCharacterResourceIdentifiers.GnosisCurrent] = 1,
            [WerewolfCharacterResourceIdentifiers.WillpowerPermanent] = 5,
            [WerewolfCharacterResourceIdentifiers.WillpowerCurrent] = 5
        };
    }

    private static Dictionary<string, int?> BuildRenown()
    {
        return new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfRenownIdentifiers.GloryPermanent] = 0,
            [WerewolfRenownIdentifiers.GloryCurrent] = 0,
            [WerewolfRenownIdentifiers.HonorPermanent] = 0,
            [WerewolfRenownIdentifiers.HonorCurrent] = 0,
            [WerewolfRenownIdentifiers.WisdomPermanent] = 0,
            [WerewolfRenownIdentifiers.WisdomCurrent] = 0
        };
    }
}
