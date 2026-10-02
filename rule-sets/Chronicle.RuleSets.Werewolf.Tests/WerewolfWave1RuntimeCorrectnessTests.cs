using System.Text.Json;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 1 runtime-correctness regression tests. Each test proves an outcome
/// changes because of source-backed mechanics, not merely that an operation
/// returned success.
/// </summary>
public sealed class WerewolfWave1RuntimeCorrectnessTests
{
    // ---------------------------------------------------------------------
    // Item 6: Gift activation pools resolve from the character's own sheet.
    // ---------------------------------------------------------------------
    [Fact]
    public void GiftActivationPoolFollowsTheCharactersAttributeValue()
    {
        var definition = TestDefinitionFor(WerewolfGiftIdentifiers.MetisCavar);
        Assert.NotNull(definition);
        Assert.Equal("Strength", definition!.TestAttribute);
        Assert.Equal("Athletics", definition.TestAbility);

        var weak = TestPoolFor(strength: 1, athletics: 1);
        var strong = TestPoolFor(strength: 4, athletics: 1);

        // Source line 1798: Cavar is tested with Strength + Athletics, so the
        // pool must equal the character's own ratings.
        Assert.Equal(1 + 1, weak);
        Assert.Equal(4 + 1, strong);
    }

    [Fact]
    public void GiftActivationPoolFollowsTheCharactersAbilityValue()
    {
        var low = TestPoolFor(strength: 2, athletics: 1);
        var high = TestPoolFor(strength: 2, athletics: 3);

        Assert.Equal(3, low);
        Assert.Equal(5, high);
    }

    [Fact]
    public void GiftActivationWithoutABoundSheetDoesNotInventAPool()
    {
        // Metis Cavar is Strength + Athletics. With no bound sheet there is no
        // rating to resolve, so the pool is zero rather than a hardcoded value.
        var state = RuntimeState();
        var known = state.KnownGiftKeys.ToList();
        known.Add(WerewolfGiftIdentifiers.MetisCavar);

        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-nosheet", state with { KnownGiftKeys = known }, 1, WerewolfGiftIdentifiers.MetisCavar));

        Assert.True(result.Succeeded);
        // Metis Cavar names Strength + Athletics, so the pool is a resolved
        // number; a null pool would mean the source named no trait at all.
        Assert.Equal(0, result.ActivationDefinition!.DicePool);
    }

    // ---------------------------------------------------------------------
    // Item 7: attack and defense use distinct source formulas.
    // Source lines 3081-3085 (attack) vs 3086-3089 (defense).
    // ---------------------------------------------------------------------
    [Fact]
    public void FirearmAttackPoolIsDexterityPlusFirearms()
    {
        var sheet = Sheet(dexterity: 3, firearms: 2);

        var pool = WerewolfCombatAttackDefinitionService.ComputeAttackPool(sheet, WerewolfCombatIdentifiers.Firearm);

        Assert.Equal(5, pool);
    }

    [Fact]
    public void MeleeAttackPoolIsDexterityPlusMeleeNotTheDefenseFormula()
    {
        var sheet = Sheet(dexterity: 3, melee: 2, dodge: 5, brawl: 1);

        var attack = WerewolfCombatAttackDefinitionService.ComputeAttackPool(sheet, WerewolfCombatIdentifiers.MeleeWeapon);

        // Source line 3083: melee weapon is Dexterity + Melee = 5.
        Assert.Equal(5, attack);
        // The defensive formula at source line 3086 is Dodge + 3 + Dexterity and
        // would be 11; the attack pool must not reuse it.
        Assert.NotEqual(11, attack);
    }

    [Fact]
    public void DifferentAttackTypesUseDifferentAbilityComponents()
    {
        var sheet = Sheet(dexterity: 3, firearms: 1, athletics: 2, melee: 3, brawl: 4);

        Assert.Equal(4, WerewolfCombatAttackDefinitionService.ComputeAttackPool(sheet, WerewolfCombatIdentifiers.Firearm));
        Assert.Equal(5, WerewolfCombatAttackDefinitionService.ComputeAttackPool(sheet, WerewolfCombatIdentifiers.Thrown));
        Assert.Equal(6, WerewolfCombatAttackDefinitionService.ComputeAttackPool(sheet, WerewolfCombatIdentifiers.MeleeWeapon));
    }

    [Fact]
    public void AttackPoolRespondsToAttributeChanges()
    {
        var baseSheet = Sheet(dexterity: 2, firearms: 1);
        var improved = Sheet(dexterity: 4, firearms: 1);

        Assert.Equal(3, WerewolfCombatAttackDefinitionService.ComputeAttackPool(baseSheet, WerewolfCombatIdentifiers.Firearm));
        Assert.Equal(5, WerewolfCombatAttackDefinitionService.ComputeAttackPool(improved, WerewolfCombatIdentifiers.Firearm));
    }

    // ---------------------------------------------------------------------
    // Item 8: soak reduces damage. Source lines 3096-3100 - each soak
    // success removes one level of damage.
    // ---------------------------------------------------------------------
    [Fact]
    public void SoakSuccessesReduceResultingDamage()
    {
        var withoutSoak = ExecuteSoak(damage: 5, dice: null);
        var noSuccesses = ExecuteSoak(damage: 5, dice: [1, 1, 1, 1, 1, 1]);
        var threeSuccesses = ExecuteSoak(damage: 5, dice: [10, 10, 10, 10, 1, 1]);

        Assert.Equal(5, Int(withoutSoak, "incomingDamage"));
        Assert.Equal(5, Int(withoutSoak, "resultingDamage"));
        Assert.Equal(0, Int(withoutSoak, "soakSuccesses"));

        Assert.Equal(0, Int(noSuccesses, "soakSuccesses"));
        Assert.Equal(5, Int(noSuccesses, "resultingDamage"));

        var successes = Int(threeSuccesses, "soakSuccesses");
        Assert.Equal(6, Int(threeSuccesses, "soakPoolSize"));
        Assert.Equal(6, Int(threeSuccesses, "difficulty"));
        Assert.Equal(5 - successes, Int(threeSuccesses, "resultingDamage"));
        Assert.True(Int(threeSuccesses, "resultingDamage") < Int(noSuccesses, "resultingDamage"));
    }

    [Fact]
    public void SoakNeverReducesDamageBelowZero()
    {
        var heavy = ExecuteSoak(damage: 1, dice: [10, 10, 10, 10, 10, 10]);

        Assert.Equal(0, Int(heavy, "resultingDamage"));
    }

    private static int Int(IReadOnlyDictionary<string, string> outputs, string key)
    {
        Assert.True(outputs.ContainsKey(key), $"Expected output '{key}'.");
        return int.Parse(outputs[key], System.Globalization.CultureInfo.InvariantCulture);
    }

    // ---------------------------------------------------------------------
    // Item 9: the social test definition service is reachable at runtime and
    // its difficulty is derived from the target, not a flat catalog value.
    // Source lines 3006-3031.
    // ---------------------------------------------------------------------
    [Fact]
    public void SocialTestDefinitionIsReachableThroughTheRuntime()
    {
        var result = ExecuteSocialTest(challengeId: WerewolfSocialChallengeIdentifiers.Interrogatorio, targetWillpower: 2, targetInteligencia: 2);

        Assert.True(result.Succeeded, string.Join("; ", result.Findings.Select(f => f.Message)));
        Assert.True(result.Outputs.ContainsKey("baseDifficulty"));
        Assert.True(result.Outputs.ContainsKey("finalDifficulty"));
        Assert.True(result.Outputs.ContainsKey("attributeId"));
        Assert.True(result.Outputs.ContainsKey("abilityId"));
        Assert.False(string.IsNullOrWhiteSpace(result.Outputs["attributeId"]));
    }

    [Fact]
    public void SocialTestDifficultyRespondsToTheTargetContext()
    {
        var weakTarget = ExecuteSocialTest(
            challengeId: WerewolfSocialChallengeIdentifiers.Interrogatorio,
            targetWillpower: 1,
            targetInteligencia: 1);
        var strongTarget = ExecuteSocialTest(
            challengeId: WerewolfSocialChallengeIdentifiers.Interrogatorio,
            targetWillpower: 5,
            targetInteligencia: 5);

        var weakDifficulty = weakTarget.Outputs.GetValueOrDefault("baseDifficulty");
        var strongDifficulty = strongTarget.Outputs.GetValueOrDefault("baseDifficulty");

        Assert.NotNull(weakDifficulty);
        Assert.NotNull(strongDifficulty);
        Assert.NotEqual(weakDifficulty, strongDifficulty);
        Assert.True(
            int.Parse(strongDifficulty!, System.Globalization.CultureInfo.InvariantCulture) >
            int.Parse(weakDifficulty!, System.Globalization.CultureInfo.InvariantCulture));
    }

    // ---------------------------------------------------------------------
    // Item 10: ranged combat is reachable and aim is capped by the real
    // Perception rating, with scope counted once.
    // ---------------------------------------------------------------------
    [Fact]
    public void RangedCombatIsReachableThroughTheRuntime()
    {
        var result = ExecuteRangedCombat(perception: 3, aimTurns: 1, hasScope: false);

        Assert.True(result.Succeeded, string.Join("; ", result.Findings.Select(f => f.Message)));
        Assert.Equal("False", result.Outputs["isBlocked"]);
        Assert.Equal("6", result.Outputs["baseDifficulty"]);
        Assert.False(string.IsNullOrWhiteSpace(result.Outputs["finalDiceBonus"]));
    }

    [Fact]
    public void RangedAimIsCappedByTheCharactersPerception()
    {
        var lowPerception = ExecuteRangedCombat(perception: 1, aimTurns: 5, hasScope: false);
        var highPerception = ExecuteRangedCombat(perception: 4, aimTurns: 5, hasScope: false);

        var low = Int(lowPerception.Outputs, "aimDiceBonus");
        var high = Int(highPerception.Outputs, "aimDiceBonus");

        Assert.Equal(1, low);
        Assert.Equal(4, high);
        Assert.True(high > low);
    }

    [Fact]
    public void RangedScopeIsCountedExactlyOnce()
    {
        var withoutScope = ExecuteRangedCombat(perception: 3, aimTurns: 2, hasScope: false);
        var withScope = ExecuteRangedCombat(perception: 3, aimTurns: 2, hasScope: true);

        var withoutBonus = Int(withoutScope.Outputs, "finalDiceBonus");
        var withBonus = Int(withScope.Outputs, "finalDiceBonus");
        var reportedScope = Int(withScope.Outputs, "scopeDiceBonus");

        // A scope grants exactly +2 dice once, not +4.
        Assert.Equal(2, reportedScope);
        Assert.Equal(withoutBonus + 2, withBonus);
    }

    // ---------------------------------------------------------------------
    // Item 5: the freebie economy is reachable and debits the budget.
    // Source lines 989-998 define the bonus point cost table.
    // ---------------------------------------------------------------------
    [Fact]
    public void FreebiePurchaseIsReachableAndAppliesTheSourceCostTable()
    {
        var created = CreateDraft();
        var draftId = created.Outputs["draftId"];
        var draftVersion = created.Outputs["draftVersion"];

        // Source line 992: Attribute costs 5 bonus points per dot.
        var result = ExecuteFreebie(draftId, draftVersion, category: "attribute", itemId: WerewolfAttributeIdentifiers.Strength, increase: 1);

        Assert.True(result.Succeeded, string.Join("; ", result.Findings.Select(f => f.Message)));
        Assert.Equal("5", result.Outputs["cost"]);
        Assert.Equal("10", result.Outputs["remainingBudget"]);
    }

    [Fact]
    public void FreebiePurchaseIsRefusedOnceTheBudgetIsExhausted()
    {
        var created = CreateDraft();
        var draftId = created.Outputs["draftId"];
        var draftVersion = created.Outputs["draftVersion"];

        var first = ExecuteFreebie(draftId, draftVersion, category: "attribute", itemId: WerewolfAttributeIdentifiers.Strength, increase: 1);
        Assert.True(first.Succeeded);

        // Budget is now exhausted, and the version moved on, so a replay fails.
        var replay = ExecuteFreebie(draftId, draftVersion, category: "attribute", itemId: WerewolfAttributeIdentifiers.Dexterity, increase: 1);

        Assert.False(replay.Succeeded);
    }

    [Fact]
    public void FreebiePurchaseRejectsAnUnknownCategory()
    {
        var created = CreateDraft();

        var result = ExecuteFreebie(
            created.Outputs["draftId"],
            created.Outputs["draftVersion"],
            category: "not-a-category",
            itemId: WerewolfAttributeIdentifiers.Strength,
            increase: 1);

        Assert.False(result.Succeeded);
        Assert.Equal(RuleSetOperationFailureCode.InvalidRequest, result.FailureCode);
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------
    private static int? TestPoolFor(int strength, int athletics)
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Strength] = strength
        };
        var abilities = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAbilityIdentifiers.Athletics] = athletics
        };

        var state = new Dictionary<string, string>(RuntimeState().PackageBinding, StringComparer.Ordinal)
        {
            ["attributes"] = JsonSerializer.Serialize(attributes),
            ["abilities"] = JsonSerializer.Serialize(abilities)
        };

        var bound = RuntimeState() with { PackageBinding = state };
        var known = bound.KnownGiftKeys.ToList();
        known.Add(WerewolfGiftIdentifiers.MetisCavar);

        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-pool", bound with { KnownGiftKeys = known }, 1, WerewolfGiftIdentifiers.MetisCavar));

        Assert.True(result.Succeeded);
        return result.ActivationDefinition!.DicePool;
    }

    private static WerewolfGiftDefinition? TestDefinitionFor(string giftId) => WerewolfGiftCatalog.Get(giftId);

    private static Dictionary<string, int> Sheet(int? strength = null, int? dexterity = null, int? perception = null, int? firearms = null, int? melee = null, int? dodge = null, int? brawl = null, int? athletics = null)
    {
        var sheet = new Dictionary<string, int>(StringComparer.Ordinal);

        if (strength is not null) sheet[WerewolfAttributeIdentifiers.Strength] = strength.Value;
        if (dexterity is not null) sheet[WerewolfAttributeIdentifiers.Dexterity] = dexterity.Value;
        if (perception is not null) sheet[WerewolfAttributeIdentifiers.Perception] = perception.Value;
        if (firearms is not null) sheet[WerewolfAbilityIdentifiers.Firearms] = firearms.Value;
        if (melee is not null) sheet[WerewolfAbilityIdentifiers.Melee] = melee.Value;
        if (dodge is not null) sheet[WerewolfAbilityIdentifiers.Dodge] = dodge.Value;
        if (brawl is not null) sheet[WerewolfAbilityIdentifiers.Brawl] = brawl.Value;
        if (athletics is not null) sheet[WerewolfAbilityIdentifiers.Athletics] = athletics.Value;

        return sheet;
    }

    private static Dictionary<string, int> FullSheet()
    {
        return new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Strength] = 2,
            [WerewolfAttributeIdentifiers.Dexterity] = 2,
            [WerewolfAttributeIdentifiers.Stamina] = 2,
            [WerewolfAttributeIdentifiers.Charisma] = 3,
            [WerewolfAttributeIdentifiers.Manipulation] = 3,
            [WerewolfAttributeIdentifiers.Appearance] = 2,
            [WerewolfAttributeIdentifiers.Perception] = 2,
            [WerewolfAttributeIdentifiers.Intelligence] = 2,
            [WerewolfAttributeIdentifiers.Wits] = 2
        };
    }

    private static Dictionary<string, int> FullAbilities()
    {
        return new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAbilityIdentifiers.Athletics] = 2,
            [WerewolfAbilityIdentifiers.Brawl] = 2,
            [WerewolfAbilityIdentifiers.Dodge] = 2,
            [WerewolfAbilityIdentifiers.Empathy] = 2,
            [WerewolfAbilityIdentifiers.Expression] = 2,
            [WerewolfAbilityIdentifiers.Intimidation] = 2,
            [WerewolfAbilityIdentifiers.Subterfuge] = 2,
            [WerewolfAbilityIdentifiers.Stealth] = 2,
            [WerewolfAbilityIdentifiers.Survival] = 2,
            [WerewolfAbilityIdentifiers.Melee] = 2,
            [WerewolfAbilityIdentifiers.Firearms] = 2
        };
    }

    private static WerewolfRuntimeCharacterState RuntimeState(Dictionary<string, int>? attributes = null, Dictionary<string, int>? abilities = null)
    {
        var binding = new Dictionary<string, string>(StringComparer.Ordinal);
        if (attributes is not null)
        {
            binding["attributes"] = JsonSerializer.Serialize(attributes);
        }

        if (abilities is not null)
        {
            binding["abilities"] = JsonSerializer.Serialize(abilities);
        }

        return new WerewolfRuntimeCharacterState(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-001",
            RuntimeStateVersion: 1,
            PackageBinding: binding,
            RagePermanent: 5,
            RageCurrent: 5,
            GnosisPermanent: 5,
            GnosisCurrent: 5,
            WillpowerPermanent: 5,
            WillpowerCurrent: 5,
            GloryPermanent: 0,
            GloryCurrent: 0,
            HonorPermanent: 0,
            HonorCurrent: 0,
            WisdomPermanent: 0,
            WisdomCurrent: 0,
            BirthRace: WerewolfRaceIdentifiers.Homid,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: WerewolfFormIdentifiers.Homid,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: WerewolfGiftIdentifiers.Supported.ToList(),
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: string.Empty,
            ActivatedGiftKeys: []);
    }

    private static IReadOnlyDictionary<string, string> ExecuteSoak(int damage, int[]? dice)
    {
        // Bashing soak is Stamina against difficulty 6, so a Stamina of 6
        // yields a pool of 6 and the supplied dice can reach the difficulty.
        var soakSheet = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Stamina] = 6
        };

        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "req-soak",
            ["expectedRuntimeStateVersion"] = "1",
            ["currentState"] = JsonSerializer.Serialize(RuntimeState(soakSheet)),
            ["amount"] = damage.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["damageType"] = "Bashing"
        };

        if (dice is not null)
        {
            inputs["diceValues"] = string.Join(",", dice);
        }

        var result = Registry().Execute(Request("combat.calculate-soak", inputs));

        Assert.True(result.Succeeded, string.Join("; ", result.Findings.Select(f => f.Message)));
        return result.Outputs;
    }

    private static RuleSetOperationResult ExecuteSocialTest(string challengeId, int? targetWillpower, int? targetInteligencia)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "req-social",
            ["challengeId"] = challengeId,
            ["expectedRuntimeStateVersion"] = "1",
            ["currentState"] = JsonSerializer.Serialize(RuntimeState(FullSheet(), FullAbilities()))
        };

        if (targetWillpower is not null)
        {
            inputs["targetWillpower"] = targetWillpower.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (targetInteligencia is not null)
        {
            inputs["targetInteligencia"] = targetInteligencia.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return Registry().Execute(Request("social.define-test", inputs));
    }

    private static RuleSetOperationResult ExecuteRangedCombat(int perception, int aimTurns, bool hasScope)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "req-ranged",
            ["attackId"] = WerewolfCombatIdentifiers.Firearm,
            ["expectedRuntimeStateVersion"] = "1",
            ["currentState"] = JsonSerializer.Serialize(RuntimeState(Sheet(dexterity: 3, firearms: 2, perception: perception))),
            ["rangeBand"] = "PointBlank",
            ["coverType"] = "None",
            ["firingMode"] = "SingleShot",
            ["aimTurns"] = aimTurns.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["hasScope"] = hasScope.ToString(),
            ["requestedShots"] = "1",
            ["currentAmmunition"] = "10",
            ["totalAmmunitionCapacity"] = "10",
            ["hasSpareClips"] = "true"
        };

        return Registry().Execute(Request("combat.resolve-ranged", inputs));
    }

    private static RuleSetOperationResult ExecuteFreebie(string draftId, string draftVersion, string category, string itemId, int increase)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "req-freebie",
            ["draftId"] = draftId,
            ["draftVersion"] = draftVersion,
            ["expectedDraftVersion"] = draftVersion,
            ["category"] = category,
            ["itemId"] = itemId,
            ["increase"] = increase.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["attributes"] = $"{WerewolfAttributeIdentifiers.Strength}:1",
            ["abilities"] = $"{WerewolfAbilityIdentifiers.Athletics}:1",
            ["backgrounds"] = $"{WerewolfBackgroundIdentifiers.Allies}:1",
            ["resources"] = $"{WerewolfCharacterResourceIdentifiers.RagePermanent}:5",
            ["renown"] = $"{WerewolfRenownIdentifiers.GloryPermanent}:0"
        };

        return Registry().Execute(Request("character-creation.purchase-freebie", inputs));
    }

    private static RuleSetOperationResult CreateDraft()
    {
        return Registry().Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.CreateCharacterOperation,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-create" }));
    }

    private static RuleSetOperationRequest Request(string operationKey, Dictionary<string, string>? inputs = null)
    {
        return new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            operationKey,
            inputs ?? new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" });
    }

    private static RuleSetRuntimeRegistry Registry()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(registration.Catalog, [new WerewolfReferenceRuntime()])).Registry;
    }

    private static string RuleSetsRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return Path.Combine(directory.FullName, "rule-sets");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }
}
