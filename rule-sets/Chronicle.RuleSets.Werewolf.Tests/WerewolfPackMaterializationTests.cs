using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfPackMaterializationTests
{
    [Fact]
    public void PackTypicalSizeIsDescriptiveNotHardConstraint()
    {
        Assert.Equal("2", WerewolfPackDefinitions.PackTypicalSizeMin);
        Assert.Equal("10", WerewolfPackDefinitions.PackTypicalSizeMax);
        Assert.Equal("generally formed by 2 to 10 Garou", WerewolfPackDefinitions.PackTypicalSizeDescription);
    }

    [Fact]
    public void PackAlphaChallengeMethodsCountIsThree()
    {
        Assert.Equal(3, WerewolfPackDefinitions.AlphaChallengeMethods.Count);
        Assert.Contains("Confrontação", WerewolfPackDefinitions.AlphaChallengeMethods);
        Assert.Contains("O Jogo", WerewolfPackDefinitions.AlphaChallengeMethods);
        Assert.Contains("Duelo", WerewolfPackDefinitions.AlphaChallengeMethods);
    }

    [Fact]
    public void PackAlphaChallengeAvailabilityHasPeaceAndWar()
    {
        Assert.True(WerewolfPackDefinitions.AlphaChallengeAvailabilityByContext.ContainsKey("peace"));
        Assert.True(WerewolfPackDefinitions.AlphaChallengeAvailabilityByContext.ContainsKey("war"));
        Assert.Equal("challenge permitted", WerewolfPackDefinitions.AlphaChallengeAvailabilityByContext["peace"]);
        Assert.Equal("challenge forbidden", WerewolfPackDefinitions.AlphaChallengeAvailabilityByContext["war"]);
    }

    [Fact]
    public void PackAuguryRolesHasFiveEntries()
    {
        Assert.Equal(5, WerewolfPackDefinitions.AuguryRoles.Count);
        Assert.Contains("Ahroun", WerewolfPackDefinitions.AuguryRoles.Keys);
        Assert.Contains("Theurge", WerewolfPackDefinitions.AuguryRoles.Keys);
        Assert.Contains("Philodox", WerewolfPackDefinitions.AuguryRoles.Keys);
        Assert.Contains("Galliard", WerewolfPackDefinitions.AuguryRoles.Keys);
        Assert.Contains("Ragabash", WerewolfPackDefinitions.AuguryRoles.Keys);
    }

    [Fact]
    public void PackLitanyHas13Rules()
    {
        Assert.Equal(13, WerewolfPackDefinitions.LitanyPackRules.Count);
    }

    [Fact]
    public void PackTacticsCatalogHasFiveEntries()
    {
        Assert.Equal(5, WerewolfPackDefinitions.PackTactics.Count);
        Assert.Contains("Arrancar Pêlos", WerewolfPackDefinitions.PackTactics.Keys);
        Assert.Contains("Cerco", WerewolfPackDefinitions.PackTactics.Keys);
        Assert.Contains("Ataque Feroz", WerewolfPackDefinitions.PackTactics.Keys);
        Assert.Contains("Osso da Sorte", WerewolfPackDefinitions.PackTactics.Keys);
        Assert.Contains("Escárnio", WerewolfPackDefinitions.PackTactics.Keys);
    }

    [Fact]
    public void PackTacticsMinimumMembersMatchSource()
    {
        Assert.Equal("2", WerewolfPackDefinitions.PackTacticsMinimumMembers["Arrancar Pêlos"]);
        Assert.Equal("4", WerewolfPackDefinitions.PackTacticsMinimumMembers["Cerco"]);
        Assert.Equal("3", WerewolfPackDefinitions.PackTacticsMinimumMembers["Ataque Feroz"]);
        Assert.Equal("2", WerewolfPackDefinitions.PackTacticsMinimumMembers["Osso da Sorte"]);
        Assert.Equal("1", WerewolfPackDefinitions.PackTacticsMinimumMembers["Escárnio"]);
    }

    [Fact]
    public void CalculateMaxTacticsReturnsMinGnosis()
    {
        Assert.Equal(0, WerewolfPackDefinitions.CalculateMaxTactics([]));
        Assert.Equal(3, WerewolfPackDefinitions.CalculateMaxTactics([3, 5, 4]));
        Assert.Equal(2, WerewolfPackDefinitions.CalculateMaxTactics([2, 7, 5]));
    }

    [Fact]
    public void PackCreationNarrativeElementsAreDescriptive()
    {
        Assert.False(string.IsNullOrWhiteSpace(WerewolfPackDefinitions.PackTypicalSizeDescription));
    }
}
