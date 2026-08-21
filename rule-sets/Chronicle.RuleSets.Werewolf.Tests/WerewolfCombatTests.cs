namespace Chronicle.RuleSets.Werewolf.Tests;

using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Linq;
using Xunit;

public class WerewolfCombatTests
{
    [Fact]
    public void InitiativeValidAttributesReturnsCorrectModifier()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(), 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, "homid", null, "character.form.homid");

        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", state, 1, 3, 2, 7, null));

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.InitiativeModifier);
        Assert.Equal(7, result.SuppliedDieRoll);
        Assert.Equal(12, result.FinalInitiative);
        Assert.Equal(3, result.Dexterity);
        Assert.Equal(2, result.Wits);
    }

    [Fact]
    public void InitiativeMaxExtraActionsCalculatesCorrectly()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(), 10, 10, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, "homid", null, "character.form.homid");

        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", state, 1, 4, 3, 2, 2));

        Assert.True(result.Succeeded);
        Assert.Equal(7, result.InitiativeModifier);
        Assert.Equal(9, result.FinalInitiative);
        Assert.Equal(3, result.MaxExtraActions);
    }

    [Fact]
    public void AttackDefinitionResolveBiteReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack("bite");

        Assert.Equal("bite", definition.AttackId);
        Assert.Equal("Strength + 1", definition.DamageExpression);
        Assert.Equal("Aggravated", definition.DamageCategory);
        Assert.True(definition.IsNaturalWeapon);
    }

    [Fact]
    public void AttackDefinitionResolveFirearmReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack("firearm");

        Assert.Equal("firearm", definition.AttackId);
        Assert.Equal("Variable", definition.DamageExpression);
        Assert.Equal("Lethal", definition.DamageCategory);
    }

    [Fact]
    public void DefenseDodgeReturnsCorrectPool()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Dodge] = 2
        };

        var result = WerewolfCombatDefenseService.ComputeDefensePool(attributes, "brawl", "dodge");

        Assert.Equal(5, result);
    }

    [Fact]
    public void DefenseFirearmBlockReturnsZeroPool()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Brawl] = 2
        };

        var result = WerewolfCombatDefenseService.ComputeDefensePool(attributes, "firearm", "block");

        Assert.Equal(0, result);
    }

    [Fact]
    public void DefenseFirearmDodgeReturnsCorrectPool()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Dodge] = 2
        };

        var result = WerewolfCombatDefenseService.ComputeDefensePool(attributes, "firearm", "dodge");

        Assert.Equal(5, result);
    }

    [Fact]
    public void DamageOneSuccessReturnsOneBaseDamage()
    {
        var definition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req", State(), 1, 1, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), null));

        var interpretation = WerewolfCombatDamageService.InterpretDamageRoll(definition, [6]);

        Assert.Equal(1, interpretation.DamageSuccesses);
        Assert.Equal(1, interpretation.TotalDamage);
    }

    [Fact]
    public void DamageThreeSuccessesReturnsThreeTotalDamage()
    {
        var definition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req", State(), 1, 3, "Strength + 1", WerewolfDamageCategory.Lethal.ToString(), null));

        var interpretation = WerewolfCombatDamageService.InterpretDamageRoll(definition, [6, 7, 8]);

        Assert.Equal(3, interpretation.DamageSuccesses);
        Assert.Equal(3, interpretation.TotalDamage);
    }

    [Fact]
    public void SoakRacialFormAggravatedAllowsSoak()
    {
        var state = State(form: "character.form.homid", birthRace: "homid");

        var definition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req", state, 1, WerewolfDamageCategory.Aggravated, 3));

        Assert.False(definition.SoakBlocked);
        Assert.True(definition.IsRacialForm);
        Assert.Equal(0, definition.SoakPoolSize);
    }

    [Fact]
    public void SoakNonRacialFormAggravatedBlocksSoakWithoutGifts()
    {
        var state = State(form: "character.form.crinos", birthRace: "homid");

        var definition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req", state, 1, WerewolfDamageCategory.Aggravated, 3));

        Assert.True(definition.SoakBlocked);
        Assert.False(definition.IsRacialForm);
        Assert.Equal(0, definition.SoakPoolSize);
    }

    [Fact]
    public void SilverNonRacialFormReturnsDamagePerTurn()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(), 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, "homid", null, "character.form.crinos");

        var result = WerewolfCombatSilverService.ApplySilverContact(state, 2, false);

        Assert.Equal(2, result);
    }

    [Fact]
    public void SilverRacialFormReturnsZeroDamage()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(), 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, "homid", null, "character.form.homid");

        var result = WerewolfCombatSilverService.ApplySilverContact(state, 2, false);

        Assert.Equal(0, result);
    }

    [Fact]
    public void RageExtraActionReturnsCorrectExtraActions()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(), 10, 10, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, "homid", null, "character.form.homid");

        var result = WerewolfCombatRageService.CalculateExtraActions(state, 3);

        Assert.NotNull(result);
        Assert.Equal(3, result.ExtraActions);
    }

    [Fact]
    public void CombatStateAddConditionReturnsUpdatedState()
    {
        var state = WerewolfCombatState.Initial(5);
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Prone);

        var updated = WerewolfCombatStateService.AddCondition(state, condition);

        Assert.Equal(2, updated.CombatStateVersion);
        Assert.Single(updated.ActiveConditions);
        Assert.Equal(WerewolfCombatConditionKind.Prone, updated.ActiveConditions[0].Kind);
    }

    [Fact]
    public void ManeuverCatalogContainsExpectedCount()
    {
        Assert.Equal(15, WerewolfCombatManeuverCatalog.Entries.Count);
    }

    [Fact]
    public void AttackCatalogContainsExpectedCount()
    {
        Assert.Equal(7, WerewolfCombatAttackCatalog.Entries.Count);
    }

    [Fact]
    public void CombatConditionBlindedHasCorrectProperties()
    {
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Blinded);

        Assert.NotNull(condition.Notes);
        Assert.Contains("Cannot dodge", condition.Notes);
    }

    private static WerewolfRuntimeCharacterState State(string form = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, form);
    }
}
