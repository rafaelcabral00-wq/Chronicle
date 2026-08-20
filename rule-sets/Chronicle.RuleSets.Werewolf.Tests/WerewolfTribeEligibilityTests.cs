using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfTribeEligibilityTests
{
    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Homid, false)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Lupus, false)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Metis, false)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Homid, false)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Metis, false)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid, false)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Lupus, false)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Metis, false)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfRaceIdentifiers.Metis, true)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfRaceIdentifiers.Homid, true)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfRaceIdentifiers.Lupus, true)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfRaceIdentifiers.Metis, true)]
    public void CheckRaceBreedEligibilityReturnsExpectedResult(string tribeId, string raceId, bool expectedEligible)
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var backgroundId in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[backgroundId] = 0;
        }

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(tribeId, raceId, backgrounds));

        if (expectedEligible)
        {
            Assert.True(result.IsEligible);
            Assert.All(result.Findings, finding => Assert.Equal(WerewolfTribeEligibilitySeverity.Information, finding.Severity));
        }
        else
        {
            Assert.False(result.IsEligible);
            Assert.Contains(result.Findings, finding => finding.Severity == WerewolfTribeEligibilitySeverity.Error);
        }
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Lupus, WerewolfTribeEligibilityErrorCode.Eligible, null)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Homid, WerewolfTribeEligibilityErrorCode.RaceBreedIneligible, "Red Talons are restricted to Lupus race per source")]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Metis, WerewolfTribeEligibilityErrorCode.RaceBreedIneligible, "Red Talons are restricted to Lupus race per source")]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Homid, WerewolfTribeEligibilityErrorCode.DependencyUnavailable, "Black Furies require female gender")]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid, WerewolfTribeEligibilityErrorCode.BackgroundMinimumNotMet, "Silver Fangs require Pure Breed >= 3")]
    public void CheckRaceBreedEligibilityProducesExpectedFinding(string tribeId, string raceId, WerewolfTribeEligibilityErrorCode expectedCode, string? expectedMessageFragment)
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var backgroundId in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[backgroundId] = 0;
        }

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(tribeId, raceId, backgrounds));

        if (StringComparer.Ordinal.Equals(expectedCode, WerewolfTribeEligibilityErrorCode.Eligible))
        {
            Assert.True(result.IsEligible);
            Assert.All(result.Findings, finding => Assert.Equal(WerewolfTribeEligibilitySeverity.Information, finding.Severity));
        }
        else
        {
            Assert.False(result.IsEligible);
            var errorFinding = result.Findings.First(finding => finding.Severity == WerewolfTribeEligibilitySeverity.Error);
            Assert.Equal(expectedCode, errorFinding.Code);
            if (!string.IsNullOrEmpty(expectedMessageFragment))
            {
                Assert.Contains(expectedMessageFragment, errorFinding.Message);
            }
        }
    }

    [Fact]
    public void SilverFangsEligibleWhenPureBreedMeetsMinimumAndAllOtherBackgroundsZero()
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfBackgroundIdentifiers.Allies] = 0,
            [WerewolfBackgroundIdentifiers.Ancestors] = 0,
            [WerewolfBackgroundIdentifiers.Contacts] = 0,
            [WerewolfBackgroundIdentifiers.Fetish] = 0,
            [WerewolfBackgroundIdentifiers.Kinfolk] = 0,
            [WerewolfBackgroundIdentifiers.Mentor] = 0,
            [WerewolfBackgroundIdentifiers.PureBreed] = 3,
            [WerewolfBackgroundIdentifiers.Resources] = 0,
            [WerewolfBackgroundIdentifiers.Rites] = 0
        };

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid, backgrounds));

        Assert.True(result.IsEligible);
    }

    [Fact]
    public void SilverFangsBackgroundMinimumNotMetWhenPureBreedBelowThree()
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfBackgroundIdentifiers.PureBreed] = 2
        };

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid, backgrounds));

        Assert.False(result.IsEligible);
        var finding = result.Findings.First(f => f.Code == WerewolfTribeEligibilityErrorCode.BackgroundMinimumNotMet);
        Assert.Contains("Pure Breed >= 3", finding.Message);
    }

    [Fact]
    public void SilverFangsEligibleWhenPureBreedMinimumMet()
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfBackgroundIdentifiers.PureBreed] = 3
        };

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid, backgrounds));

        Assert.True(result.IsEligible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("glass walkers")]
    public void RejectsUnknownOrMalformedTribe(string tribeId)
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var backgroundId in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[backgroundId] = 0;
        }

        var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(tribeId, WerewolfRaceIdentifiers.Homid, backgrounds));

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeEligibilityErrorCode.UnknownTribe);
    }

    [Fact]
    public void AllTwelveCanonicalTribesAreRecognized()
    {
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var backgroundId in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[backgroundId] = 0;
        }

        foreach (var tribeId in WerewolfTribeIdentifiers.Supported)
        {
            var result = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(tribeId, WerewolfRaceIdentifiers.Homid, backgrounds));
            Assert.NotEqual(WerewolfTribeEligibilityErrorCode.UnknownTribe, result.Findings.FirstOrDefault(f => f.Code == WerewolfTribeEligibilityErrorCode.UnknownTribe)?.Code);
        }
    }

    [Fact]
    public void NoDuplicateKeysOrOrphanRestrictions()
    {
        var tribes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tribeId in WerewolfTribeIdentifiers.Supported)
        {
            Assert.True(tribes.Add(tribeId), $"Duplicate Tribe key: {tribeId}");
        }

        var restrictions = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tribeId in WerewolfTribeIdentifiers.Supported)
        {
            var raceFindings = WerewolfTribeEligibilityService.CheckRaceBreedEligibility(tribeId, WerewolfRaceIdentifiers.Homid);
            var backgroundFindings = WerewolfTribeEligibilityService.CheckBackgroundMinimums(tribeId, new Dictionary<string, int?>(StringComparer.Ordinal));
            var dependencyFindings = WerewolfTribeEligibilityService.CheckDependencies(tribeId, new Dictionary<string, int?>(StringComparer.Ordinal));

            foreach (var finding in raceFindings.Concat(backgroundFindings).Concat(dependencyFindings))
            {
                var key = $"{tribeId}:{finding.Code}";
                Assert.True(restrictions.TryAdd(key, 1), $"Duplicate restriction: {key}");
            }
        }
    }
}
