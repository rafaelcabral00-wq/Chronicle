using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Domain integrity of the Tribe Gift table: the audit must cover every
/// catalogued Tribe Gift exactly once, every one of them must resolve, and the
/// Breed and Auspice tables must be untouched by it.
/// </summary>
/// <remarks>
/// Where a test in this class says it verifies the table rather than the
/// runtime, that distinction is deliberate and load-bearing: this wave routes
/// only executable and blocked Tribe Gifts through the table, so a table-level
/// guarantee over deferred Gifts has no runtime counterpart.
/// </remarks>
public sealed class WerewolfTribeGiftClosureTests
{
    private const string GenericBreedReason = "Blocked: prerequisite subsystem is not implemented.";

    private static readonly string[] TribeStatusNames = ["Executable", "Blocked", "Deferred"];
    private static readonly string[] SharedStatusNames = ["Executable", "Blocked"];
    private static readonly string[] DurationTypeNames = ["Instant", "Scene", "Permanent", "Turn"];

    // =====================================================================
    // 1. Coverage and counts.
    // =====================================================================

    [Fact]
    public void TheTribeTableCoversEveryCataloguedTribeGiftExactlyOnce()
    {
        var catalogKeys = WerewolfGiftCatalog.AllDefinitions
            .Where(definition => definition.Category == WerewolfGiftCategory.Tribe)
            .Select(definition => definition.GiftKey)
            .ToList();

        Assert.Equal(132, catalogKeys.Count);
        Assert.Equal(132, WerewolfTribeGiftMechanics.TribeGiftKeys.Count);
        Assert.Equal(132, WerewolfTribeGiftMechanics.TribeGiftKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            catalogKeys.ToHashSet(StringComparer.Ordinal),
            WerewolfTribeGiftMechanics.TribeGiftKeys.ToHashSet(StringComparer.Ordinal));
    }

    [Fact]
    public void TheAuditCoversExactlyOneHundredAndThirtyTwoGiftsSplitFourSevenOneHundredAndTwentyOne()
    {
        var audit = WerewolfTribeGiftMechanics.Audit();

        Assert.Equal(132, audit.Count);
        Assert.Equal(132, audit.Select(entry => entry.GiftKey).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(4, audit.Count(entry => entry.Status == WerewolfTribeGiftStatus.Executable));
        Assert.Equal(7, audit.Count(entry => entry.Status == WerewolfTribeGiftStatus.Blocked));
        Assert.Equal(121, audit.Count(entry => entry.Status == WerewolfTribeGiftStatus.Deferred));
    }

    [Fact]
    public void EveryAuditedGiftResolvesToACatalogDefinitionWithItsSourceLocator()
    {
        foreach (var entry in WerewolfTribeGiftMechanics.Audit())
        {
            Assert.NotNull(WerewolfGiftCatalog.Get(entry.GiftKey));
            Assert.StartsWith("Line ", entry.SourceLocator, StringComparison.Ordinal);
            Assert.Equal(
                WerewolfGiftCatalog.Get(entry.GiftKey)!.SourceLocator,
                entry.SourceLocator);
            Assert.False(string.IsNullOrWhiteSpace(entry.NameEn));
            Assert.False(string.IsNullOrWhiteSpace(entry.NamePtBr));
            Assert.InRange(entry.Level, 1, 5);
        }
    }

    [Fact]
    public void TheThreeStatusListsPartitionTheTribeGiftsWithoutOverlap()
    {
        var executable = WerewolfTribeGiftMechanics.ExecutableGiftKeys;
        var blocked = WerewolfTribeGiftMechanics.BlockedGiftKeys;
        var deferred = WerewolfTribeGiftMechanics.DeferredGiftKeys;

        Assert.Empty(executable.Intersect(blocked, StringComparer.Ordinal));
        Assert.Empty(executable.Intersect(deferred, StringComparer.Ordinal));
        Assert.Empty(blocked.Intersect(deferred, StringComparer.Ordinal));
        Assert.Equal(132, executable.Count + blocked.Count + deferred.Count);
    }

    // =====================================================================
    // 2. Every registered key resolves, and a foreign key does not.
    // =====================================================================

    /// <summary>
    /// <para>
    /// This test verifies the mechanics TABLE, not runtime reachability.
    /// WerewolfGiftEffectService.ApplyEffect is the only production caller of
    /// WerewolfTribeGiftMechanics.Resolve and it calls it only when
    /// <c>IsExecutable</c> or <c>IsBlocked</c> is true, so the Deferred arm of
    /// Resolve is unreachable in production. Calling Resolve directly here proves
    /// the table is total and internally consistent; it proves nothing about what
    /// a running game does with a deferred Tribe Gift.
    /// </para>
    /// <para>
    /// Runtime reachability for this wave is covered separately, and only for the
    /// 11 Glass Walkers Gifts, in WerewolfTribeGiftTests.
    /// </para>
    /// </summary>
    [Fact]
    public void TheTableResolvesEveryRegisteredTribeGiftToARealMechanic()
    {
        foreach (var giftKey in WerewolfTribeGiftMechanics.TribeGiftKeys)
        {
            foreach (var successes in new[] { 0, 1, 5 })
            {
                var mechanic = WerewolfTribeGiftMechanics.Resolve(
                    giftKey, successes, WerewolfFormIdentifiers.Crinos, EmptySheet(), permanentGlory: 2);

                Assert.NotNull(mechanic);
                Assert.Equal(giftKey, mechanic!.GiftKey);
                Assert.Equal(WerewolfGiftCatalog.Get(giftKey)!.SourceLocator, mechanic.SourceLocator);
                Assert.StartsWith("Line ", mechanic.SourceLocator, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void AnExecutableTribeGiftNeverResolvesToTheBlockedPayload()
    {
        foreach (var giftKey in WerewolfTribeGiftMechanics.ExecutableGiftKeys)
        {
            var mechanic = WerewolfTribeGiftMechanics.Resolve(
                giftKey, 1, WerewolfFormIdentifiers.Homid, EmptySheet(), permanentGlory: 1);

            Assert.NotNull(mechanic);
            Assert.IsNotType<WerewolfTribeGiftBlockedPayload>(mechanic!.Payload);
            Assert.NotNull(mechanic.Payload);
            Assert.StartsWith("Line ", mechanic.SourceLocator, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AGiftFromAnotherCategoryIsNotAValidTribeInput()
    {
        Assert.Null(WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1, WerewolfFormIdentifiers.Homid, EmptySheet()));
        Assert.Null(WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.RagabashOpenSeal, 1, WerewolfFormIdentifiers.Homid, EmptySheet()));
        Assert.Null(WerewolfTribeGiftMechanics.Resolve(
            "gift.unknown", 1, WerewolfFormIdentifiers.Homid, EmptySheet()));

        Assert.False(WerewolfTribeGiftMechanics.IsTribeGift(WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.Throws<ArgumentException>(
            () => WerewolfTribeGiftMechanics.StatusOf(WerewolfGiftIdentifiers.HomidMasterOfFire));
    }

    // =====================================================================
    // 3. No non-executable Gift is quietly catalog-only.
    // =====================================================================

    /// <summary>
    /// <para>
    /// This test verifies the mechanics TABLE, not runtime reachability. Only
    /// blocked Tribe Gifts reach Resolve from production; the 121 deferred Gifts
    /// do not, so their non-generic reason is a property of the table rather than
    /// of any runtime behaviour. A Gift whose reason regressed to a generic string
    /// would fail here even though no runtime path would ever surface it.
    /// </para>
    /// </summary>
    [Fact]
    public void TheTableGivesEveryNonExecutableGiftAnHonestNonGenericReason()
    {
        foreach (var giftKey in WerewolfTribeGiftMechanics.BlockedGiftKeys.Concat(WerewolfTribeGiftMechanics.DeferredGiftKeys))
        {
            var mechanic = WerewolfTribeGiftMechanics.Resolve(
                giftKey, 2, WerewolfFormIdentifiers.Homid, EmptySheet());

            Assert.Equal(WerewolfActiveGiftEffectKind.Custom, mechanic!.Kind);
            var payload = Assert.IsType<WerewolfTribeGiftBlockedPayload>(mechanic.Payload);

            Assert.False(string.IsNullOrWhiteSpace(payload.Reason));
            Assert.False(string.IsNullOrWhiteSpace(payload.MissingSubsystem));
            Assert.NotEqual(GenericBreedReason, payload.Reason);
            Assert.NotEqual("unknown", payload.MissingSubsystem);
            Assert.NotEqual("Blocked: prerequisite subsystem is not implemented.", payload.Reason);
            Assert.Equal(
                WerewolfGiftCatalog.Get(giftKey)!.SourceLocator,
                payload.SourceLocator);
        }
    }

    [Fact]
    public void BlockedGiftsNameTheMissingDependencyAndDeferredGiftsDoNotClaimOne()
    {
        foreach (var giftKey in WerewolfTribeGiftMechanics.BlockedGiftKeys)
        {
            var (status, reason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);

            Assert.Equal(WerewolfTribeGiftStatus.Blocked, status);
            Assert.NotNull(reason);
            Assert.False(string.IsNullOrWhiteSpace(reason));
            Assert.NotEqual("unknown", reason);
        }

        foreach (var giftKey in WerewolfTribeGiftMechanics.DeferredGiftKeys)
        {
            var audit = WerewolfTribeGiftMechanics.Audit().Single(entry => entry.GiftKey == giftKey);

            Assert.Equal(WerewolfTribeGiftStatus.Deferred, audit.Status);
            Assert.Null(audit.MissingDependency);
        }
    }

    [Fact]
    public void StatusOfReportsExecutableGiftsWithoutAnyReason()
    {
        foreach (var giftKey in WerewolfTribeGiftMechanics.ExecutableGiftKeys)
        {
            var (status, reason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);

            Assert.Equal(WerewolfTribeGiftStatus.Executable, status);
            Assert.Null(reason);
        }
    }

    [Fact]
    public void TheStatusEnumIsTribeLocalAndExactlyThreeValues()
    {
        // WerewolfTribeGiftStatus is Tribe-local on purpose: the shared Breed
        // status enum must keep exactly two members, and the shared duration
        // enum must not gain a Day member that both duration engines would
        // resolve to zero turns.
        Assert.Equal(TribeStatusNames, Enum.GetNames<WerewolfTribeGiftStatus>());
        Assert.Equal(SharedStatusNames, Enum.GetNames<WerewolfBreedGiftStatus>());
        Assert.Equal(DurationTypeNames, Enum.GetNames<WerewolfGiftDurationType>());
    }

    // =====================================================================
    // 4. The Breed and Auspice tables are unchanged by this wave.
    // =====================================================================

    [Fact]
    public void BreedAndAuspiceKeySetsAndStatusesAreUnchanged()
    {
        Assert.Equal(33, WerewolfGiftCatalog.AllDefinitions.Count(g => g.Category == WerewolfGiftCategory.Breed));
        Assert.Equal(60, WerewolfGiftCatalog.AllDefinitions.Count(g => g.Category == WerewolfGiftCategory.Auspice));
        Assert.Equal(33, WerewolfBreedGiftMechanics.BreedGiftKeys.Count);
        Assert.Equal(60, WerewolfAuspiceGiftMechanics.AuspiceGiftKeys.Count);

        Assert.All(WerewolfBreedGiftMechanics.BreedGiftKeys, key => Assert.False(WerewolfTribeGiftMechanics.IsTribeGift(key)));
        Assert.All(WerewolfAuspiceGiftMechanics.AuspiceGiftKeys, key => Assert.False(WerewolfTribeGiftMechanics.IsTribeGift(key)));

        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            var (status, _) = WerewolfBreedGiftMechanics.StatusOf(giftKey);
            Assert.True(status is WerewolfBreedGiftStatus.Executable or WerewolfBreedGiftStatus.Blocked);
        }

        foreach (var giftKey in WerewolfAuspiceGiftMechanics.AuspiceGiftKeys)
        {
            var (status, _) = WerewolfAuspiceGiftMechanics.StatusOf(giftKey);
            Assert.True(status is WerewolfBreedGiftStatus.Executable or WerewolfBreedGiftStatus.Blocked);
        }
    }

    [Fact]
    public void BreedAndAuspiceGiftsStillResolveThroughTheirOwnTables()
    {
        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            Assert.NotNull(WerewolfBreedGiftMechanics.Resolve(
                giftKey, 1, WerewolfFormIdentifiers.Crinos, EmptySheet()));
            Assert.Null(WerewolfTribeGiftMechanics.Resolve(
                giftKey, 1, WerewolfFormIdentifiers.Crinos, EmptySheet()));
        }

        foreach (var giftKey in WerewolfAuspiceGiftMechanics.AuspiceGiftKeys)
        {
            Assert.NotNull(WerewolfAuspiceGiftMechanics.Resolve(
                giftKey, 1, WerewolfFormIdentifiers.Crinos, new Dictionary<string, int>(StringComparer.Ordinal)));
            Assert.Null(WerewolfTribeGiftMechanics.Resolve(
                giftKey, 1, WerewolfFormIdentifiers.Crinos, EmptySheet()));
        }
    }

    // =====================================================================
    // 5. The package still declares what it cannot execute.
    // =====================================================================

    [Fact]
    public void RuntimeGiftExecutionIsStillDeclaredDisabled()
    {
        // Executing Gift effects at runtime is not advertised as a capability,
        // so no test may pass because a capability claim was relaxed.
        Assert.Contains("runtime-gift-execution", WerewolfRuleSetPackage.DisabledCapabilities);
        Assert.DoesNotContain("runtime-gift-execution", WerewolfRuleSetPackage.SupportedCapabilities);
    }

    [Fact]
    public void TheTribeMechanicsFileIsDeclaredByThePackageSourceValidator()
    {
        // An undeclared package source file fails validation, so the allowlist
        // must carry the new file or the package stops validating at all.
        var result = RuleSetPackageSourceValidator.Validate(PackageRoot());

        Assert.True(
            result.IsValid,
            string.Join("; ", result.Findings.Select(finding => finding.Code + ":" + finding.Message)));
        Assert.Contains(
            result.FileInventory,
            entry => entry.EndsWith("WerewolfTribeGiftMechanics.cs", StringComparison.Ordinal));
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static string PackageRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "rule-sets", "Chronicle.RuleSets.Werewolf");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find Werewolf package source root.");
    }

    private static Dictionary<string, int> EmptySheet() => new(StringComparer.Ordinal);
}