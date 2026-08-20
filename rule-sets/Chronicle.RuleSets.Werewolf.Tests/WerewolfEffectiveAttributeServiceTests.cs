using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfEffectiveAttributeServiceTests
{
    private static readonly IReadOnlyDictionary<string, int?> BaseAttributes = new Dictionary<string, int?>(StringComparer.Ordinal)
    {
        [WerewolfAttributeIdentifiers.Strength] = 3,
        [WerewolfAttributeIdentifiers.Dexterity] = 2,
        [WerewolfAttributeIdentifiers.Stamina] = 3,
        [WerewolfAttributeIdentifiers.Charisma] = 2,
        [WerewolfAttributeIdentifiers.Manipulation] = 2,
        [WerewolfAttributeIdentifiers.Appearance] = 2,
        [WerewolfAttributeIdentifiers.Perception] = 3,
        [WerewolfAttributeIdentifiers.Intelligence] = 2,
        [WerewolfAttributeIdentifiers.Wits] = 3
    };

    [Fact]
    public void HomidPreservesBaseAttributes()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Homid);

        Assert.Equal(3, effective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(3, effective[WerewolfAttributeIdentifiers.Stamina]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Manipulation]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Appearance]);
    }

    [Fact]
    public void GlabroAddsStrengthStaminaAndReducesManipulationAppearance()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Glabro);

        Assert.Equal(5, effective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(5, effective[WerewolfAttributeIdentifiers.Stamina]);
        Assert.Equal(0, effective[WerewolfAttributeIdentifiers.Manipulation]);
        Assert.Equal(1, effective[WerewolfAttributeIdentifiers.Appearance]);
    }

    [Fact]
    public void CrinosAddsStrengthDexterityStaminaAndZerosManipulationAppearance()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Crinos);

        Assert.Equal(7, effective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(3, effective[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(6, effective[WerewolfAttributeIdentifiers.Stamina]);
        Assert.Equal(0, effective[WerewolfAttributeIdentifiers.Manipulation]);
        Assert.Equal(0, effective[WerewolfAttributeIdentifiers.Appearance]);
    }

    [Fact]
    public void HispoAddsStrengthDexterityStaminaAndZerosManipulation()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Hispo);

        Assert.Equal(6, effective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(4, effective[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(6, effective[WerewolfAttributeIdentifiers.Stamina]);
        Assert.Equal(0, effective[WerewolfAttributeIdentifiers.Manipulation]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Appearance]);
    }

    [Fact]
    public void LupusAddsStrengthDexterityStaminaAndZerosManipulation()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Lupus);

        Assert.Equal(4, effective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(4, effective[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(5, effective[WerewolfAttributeIdentifiers.Stamina]);
        Assert.Equal(0, effective[WerewolfAttributeIdentifiers.Manipulation]);
        Assert.Equal(2, effective[WerewolfAttributeIdentifiers.Appearance]);
    }

    [Fact]
    public void EffectiveAttributeDoesNotPermanentlyMutateBaseAttributes()
    {
        var baseCopy = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Strength] = 3
        };

        WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(baseCopy, WerewolfFormIdentifiers.Crinos);

        Assert.Equal(3, baseCopy[WerewolfAttributeIdentifiers.Strength]);
    }

    [Fact]
    public void RevertingFormRestoresBaseEffectiveValues()
    {
        var crinosEffective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Crinos);
        var homidEffective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, WerewolfFormIdentifiers.Homid);

        Assert.Equal(7, crinosEffective[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(3, homidEffective[WerewolfAttributeIdentifiers.Strength]);
    }

    [Fact]
    public void InvalidFormReturnsEmptyDictionary()
    {
        var effective = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(BaseAttributes, "invalid-form");
        Assert.Empty(effective);
    }

    [Fact]
    public void GetEffectiveAttributeReturnsCorrectValue()
    {
        var effective = WerewolfEffectiveAttributeService.GetEffectiveAttribute(BaseAttributes, WerewolfFormIdentifiers.Crinos, WerewolfAttributeIdentifiers.Strength);
        Assert.Equal(7, effective);
    }

    [Fact]
    public void GetEffectiveAttributeReturnsZeroForUnknownAttribute()
    {
        var effective = WerewolfEffectiveAttributeService.GetEffectiveAttribute(BaseAttributes, WerewolfFormIdentifiers.Crinos, "unknown-attribute");
        Assert.Equal(0, effective);
    }
}
