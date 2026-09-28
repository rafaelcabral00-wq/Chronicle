using System.Text.Json;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 1 source-fidelity regression tests. Every assertion in this file is
/// traceable to a specific line range in
/// <c>.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt</c>.
/// </summary>
public sealed class WerewolfWave1SourceFidelityTests
{
    private static readonly int[] NoDice = [];

    // ---------------------------------------------------------------------
    // Item 1: Caern Película table fidelity (source lines 3249-3255)
    // | Nível do Caern | Nível da Película | Distância Máxima da Ponte da Lua |
    // | 1 | 4 | 1.500 km | 2 | 4 | 3.000 km | 3 | 3 | 5.000 km |
    // | 4 | 3 | 9.500 km | 5 | 2 | 15.000 km |
    // ---------------------------------------------------------------------
    public static TheoryData<int, int, int> CaernTable => new()
    {
        { 1, 4, 1500 },
        { 2, 4, 3000 },
        { 3, 3, 5000 },
        { 4, 3, 9500 },
        { 5, 2, 15000 }
    };

    [Theory]
    [MemberData(nameof(CaernTable))]
    public void CaernPelículaRuntimeMatchesSourceTableExactly(int caernLevel, int expectedPelícula, int expectedMoonBridgeKm)
    {
        var registry = RegisteredRuntimeRegistry();

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "spirit-umbra.caern-película",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-caern-fidelity",
                ["caernLevel"] = caernLevel.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }));

        Assert.True(result.Succeeded);

        var boundary = DeserializeBoundary(result);
        Assert.Equal(caernLevel, boundary.CaernLevel);
        Assert.Equal(expectedPelícula, boundary.PelículaLevel);
        Assert.Equal(expectedMoonBridgeKm, boundary.MoonBridgeMaxDistanceKm);
    }

    [Fact]
    public void CaernPelículaCitesTheSourceTableItIsDerivedFrom()
    {
        var registry = RegisteredRuntimeRegistry();

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "spirit-umbra.caern-película",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-caern-locator",
                ["caernLevel"] = "1"
            }));

        var boundary = DeserializeBoundary(result);
        Assert.Contains("3249", boundary.SourceLocator, StringComparison.Ordinal);
        Assert.Contains("3255", boundary.SourceLocator, StringComparison.Ordinal);
    }

    [Fact]
    public void CaernPelículaNeverReportsZeroPelículaBecauseSourceFloorIsTwo()
    {
        // Source line 3235 constrains Gauntlet to 2-9; the source Caern table's
        // minimum Película level is 2 (level 5). A level-5 caern reporting 0
        // would make the Gauntlet roll impossible, which the source forbids.
        var registry = RegisteredRuntimeRegistry();

        for (var level = 1; level <= 5; level++)
        {
            var result = registry.Execute(new RuleSetOperationRequest(
                WerewolfRuleSetPackage.ProvisionalPackageId,
                WerewolfRuleSetPackage.PackageVersion,
                "spirit-umbra.caern-película",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["requestId"] = "req-caern-floor",
                    ["caernLevel"] = level.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }));

            var boundary = DeserializeBoundary(result);
            Assert.InRange(boundary.PelículaLevel, 2, 4);
        }
    }

    private static WerewolfCaernPelículaBoundaryPayload DeserializeBoundary(RuleSetOperationResult result)
    {
        var json = result.Outputs["boundary"];
        return JsonSerializer.Deserialize<WerewolfCaernPelículaBoundaryPayload>(json)
            ?? throw new InvalidOperationException("Caern boundary payload deserialised to null.");
    }

    private static RuleSetRuntimeRegistry RegisteredRuntimeRegistry()
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
