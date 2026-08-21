using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatStateTests
{
    [Fact]
    public void AddConditionProneIncrementsVersionAndAddsCondition()
    {
        var state = WerewolfCombatState.Initial(5);
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Prone);

        var updated = WerewolfCombatStateService.AddCondition(state, condition);

        Assert.Equal(2, updated.CombatStateVersion);
        Assert.Single(updated.ActiveConditions);
        Assert.Equal(WerewolfCombatConditionKind.Prone, updated.ActiveConditions[0].Kind);
    }

    [Fact]
    public void AddConditionBlindedAddsCorrectProperties()
    {
        var state = WerewolfCombatState.Initial(5);
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Blinded);

        var updated = WerewolfCombatStateService.AddCondition(state, condition);

        Assert.Single(updated.ActiveConditions);
        Assert.Equal(WerewolfCombatConditionKind.Blinded, updated.ActiveConditions[0].Kind);
        Assert.Equal("Line 3107", updated.ActiveConditions[0].SourceLocator);
    }

    [Fact]
    public void AddConditionMultipleConditionsAccumulates()
    {
        var state = WerewolfCombatState.Initial(5);
        var prone = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Prone);
        var stunned = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Stunned);

        var updated = WerewolfCombatStateService.AddCondition(state, prone);
        updated = WerewolfCombatStateService.AddCondition(updated, stunned);

        Assert.Equal(3, updated.CombatStateVersion);
        Assert.Equal(2, updated.ActiveConditions.Count);
        Assert.Equal(WerewolfCombatConditionKind.Prone, updated.ActiveConditions[0].Kind);
        Assert.Equal(WerewolfCombatConditionKind.Stunned, updated.ActiveConditions[1].Kind);
    }

    [Fact]
    public void AddConditionNullConditionThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WerewolfCombatStateService.AddCondition(WerewolfCombatState.Initial(5), null!));
    }

    [Fact]
    public void AddConditionNullStateThrowsArgumentNullException()
    {
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Prone);
        Assert.Throws<ArgumentNullException>(() => WerewolfCombatStateService.AddCondition(null!, condition));
    }

    [Fact]
    public void CombatStateInitialReturnsCorrectDefaults()
    {
        var state = WerewolfCombatState.Initial(5);

        Assert.Equal(1, state.CombatStateVersion);
        Assert.Equal(5, state.InitiativeDicePool);
        Assert.Equal(0, state.InitiativeScore);
        Assert.Equal(0, state.ExtraActionsAvailable);
        Assert.Equal(0, state.RageInvestedInInitiative);
        Assert.False(state.RageInvestedInTransformation);
        Assert.False(state.RageInvestedInPainNegation);
        Assert.Empty(state.ActiveConditions);
        Assert.Equal(0, state.CurrentActionCount);
        Assert.Equal(0, state.TurnNumber);
    }

    [Fact]
    public void CombatStateImmutableAddConditionDoesNotMutateOriginal()
    {
        var state = WerewolfCombatState.Initial(5);
        var condition = WerewolfCombatConditionCatalog.Entries.First(c => c.Kind == WerewolfCombatConditionKind.Prone);

        var updated = WerewolfCombatStateService.AddCondition(state, condition);

        Assert.Empty(state.ActiveConditions);
        Assert.Equal(1, state.CombatStateVersion);
        Assert.Single(updated.ActiveConditions);
    }

    [Fact]
    public void ConditionCatalogContainsExpectedEntries()
    {
        var entries = WerewolfCombatConditionCatalog.Entries;
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Healthy);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Wounded);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Incapacitated);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Unconscious);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.NearDeath);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Dead);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Prone);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Grappled);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Immobilized);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Stunned);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.Blinded);
        Assert.Contains(entries, c => c.Kind == WerewolfCombatConditionKind.SilverContact);
    }

    [Fact]
    public void ConditionCatalogAllEntriesHaveSourceLocators()
    {
        foreach (var condition in WerewolfCombatConditionCatalog.Entries)
        {
            Assert.False(string.IsNullOrWhiteSpace(condition.SourceLocator));
            Assert.False(string.IsNullOrWhiteSpace(condition.Notes));
        }
    }
}

