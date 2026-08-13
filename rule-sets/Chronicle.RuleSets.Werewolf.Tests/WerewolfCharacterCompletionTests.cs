using System.Text.Json;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCharacterCompletionTests
{
    [Fact]
    public void CompleteHomidPathSucceedsWithoutRenown()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        if (!result.Succeeded)
        {
            var messages = string.Join(Environment.NewLine, result.Findings.Select(f => $"{f.Code}: {f.Message}"));
            Assert.True(result.Succeeded, $"Completion failed:{Environment.NewLine}{messages}");
        }
        Assert.NotNull(result.Snapshot);
        Assert.Equal(WerewolfCharacterDraftStatus.Completed, result.Draft?.Status);
        Assert.Equal(WerewolfCharacterDraftStatus.Completed, result.Snapshot!.Status);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.Empty(result.Draft!.RequiredNextSteps);
    }

    [Fact]
    public void CompleteMetisPathSucceedsWithDeformity()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Philodox, WerewolfMetisDeformityIdentifiers.Horns);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Snapshot);
        Assert.Equal(WerewolfCharacterDraftStatus.Completed, result.Draft?.Status);
        Assert.Equal(WerewolfMetisDeformityIdentifiers.Horns, result.Snapshot!.MetisDeformity);
    }

    [Fact]
    public void CompleteLupusPathSucceeds()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Galliard);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Snapshot);
        Assert.Equal(WerewolfCharacterDraftStatus.Completed, result.Draft?.Status);
    }

    [Theory]
    [InlineData(WerewolfAuspiceIdentifiers.Ragabash)]
    [InlineData(WerewolfAuspiceIdentifiers.Philodox)]
    public void RagabashAndPhilodoxCompleteWithoutRenown(string auspice)
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, auspice);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.True(result.Succeeded, Format(result.Findings));
    }

    [Fact]
    public void RejectsMissingRace()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { Race = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.RaceMissing);
    }

    [Fact]
    public void RejectsMissingAuspice()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { Auspice = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AuspiceMissing);
    }

    [Fact]
    public void RejectsMissingTribe()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { Tribe = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.TribeMissing);
    }

    [Fact]
    public void RejectsMissingMetisDeformityWhenMetis()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Ragabash, null);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.MetisDeformityMissing);
    }

    [Fact]
    public void RejectsMissingRaceGift()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { RaceGift = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.RaceGiftMissing);
    }

    [Fact]
    public void RejectsMissingAuspiceGift()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { AuspiceGift = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AuspiceGiftMissing);
    }

    [Fact]
    public void RejectsMissingTribeGift()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { TribeGift = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.TribeGiftMissing);
    }

    [Fact]
    public void RejectsMissingAttributePriorities()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { AttributePriorityOrder = [] };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AttributePrioritiesMissing);
    }

    [Fact]
    public void RejectsIncompleteAttributeAllocation()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 1
            }
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AttributeAllocationIncomplete);
    }

    [Fact]
    public void RejectsMissingAbilityPriorities()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { AbilityPriorityOrder = [] };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AbilityPrioritiesMissing);
    }

    [Fact]
    public void RejectsIncompleteAbilityAllocation()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Alertness] = 0
            }
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.AbilityAllocationIncomplete);
    }

    [Fact]
    public void RejectsIncompleteBackgroundAllocation()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 0
            }
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.BackgroundAllocationIncomplete);
    }

    [Fact]
    public void RejectsMissingResources()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Resources = new Dictionary<string, int?>(StringComparer.Ordinal)
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.ResourcesNotInitialized);
    }

    [Fact]
    public void RejectsMissingRank()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { Rank = null };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.RankNotInitialized);
    }

    [Fact]
    public void RejectsStaleDraftVersion()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion - 1));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void RejectsNullDraft()
    {
        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(null!, 1));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.MissingDraft);
    }

    [Fact]
    public void RejectsAlreadyCompletedDraft()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with { Status = WerewolfCharacterDraftStatus.Completed };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.DraftAlreadyCompleted);
    }

    [Fact]
    public void SourceDraftRemainsUnchangedOnFailure()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);
        var originalVersion = draft.DraftVersion;

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion - 1));

        Assert.False(result.Succeeded);
        Assert.Equal(originalVersion, draft.DraftVersion);
        Assert.Equal(WerewolfCharacterDraftStatus.Initialized, draft.Status);
    }

    [Fact]
    public void FindingsAreDeterministicallyOrdered()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Race = null,
            Auspice = null,
            Tribe = null,
            IdentityName = null
        };

        var first = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));
        var second = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.Equal(first.Findings.Select(f => f.Code), second.Findings.Select(f => f.Code));
    }

    [Fact]
    public void SnapshotIsDeterministicForSameInput()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var first = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));
        var second = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.NotNull(first.Snapshot);
        Assert.NotNull(second.Snapshot);
        Assert.Equal(ComputeSnapshotFingerprint(first.Snapshot), ComputeSnapshotFingerprint(second.Snapshot));
    }

    [Fact]
    public void SnapshotRetainsPackageBinding()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.NotNull(result.Snapshot);
        Assert.Equal(WerewolfRuleSetPackage.ProvisionalPackageId, result.Snapshot!.PackageBinding["packageId"]);
        Assert.Equal(WerewolfRuleSetPackage.PackageVersion, result.Snapshot!.PackageBinding["packageVersion"]);
        Assert.Equal(WerewolfRuleSetPackage.DeclaredReleaseScope, result.Snapshot!.PackageBinding["declaredReleaseScope"]);
        Assert.Equal("1", result.Snapshot!.PackageBinding["contractVersion"]);
        Assert.Equal(draft.DraftVersion + 1, result.Snapshot.DraftVersion);
    }

    [Fact]
    public void CompletionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfCharacterCompletion.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void RuntimeRegistryInvokesCompleteCharacter()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-completion" }));
        Assert.True(created.Succeeded, "CreateCharacter failed: " + string.Join("; ", created.Findings.Select(f => f.Code + ":" + f.Message)));

        var raceSelected = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = created.Outputs["draftId"],
            ["draftVersion"] = created.Outputs["draftVersion"],
            ["expectedDraftVersion"] = created.Outputs["draftVersion"],
            ["raceId"] = WerewolfRaceIdentifiers.Homid
        }));
        Assert.True(raceSelected.Succeeded, "SelectRace failed: " + string.Join("; ", raceSelected.Findings.Select(f => f.Code + ":" + f.Message)));

        var auspiceSelected = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, Inputs(raceSelected.Outputs, ("auspiceId", WerewolfAuspiceIdentifiers.Ragabash))));
        Assert.True(auspiceSelected.Succeeded);

        var tribeSelected = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, Inputs(auspiceSelected.Outputs, ("tribeId", WerewolfTribeIdentifiers.GlassWalkers))));
        Assert.True(tribeSelected.Succeeded);

        var raceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceGiftOperation, Inputs(tribeSelected.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.HomidMasterOfFire))));
        Assert.True(raceGift.Succeeded, "SelectRaceGift failed: " + string.Join("; ", raceGift.Findings.Select(f => f.Code + ":" + f.Message)));

        var auspiceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceGiftOperation, Inputs(raceGift.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.RagabashOpenSeal))));
        Assert.True(auspiceGift.Succeeded, "SelectAuspiceGift failed: " + string.Join("; ", auspiceGift.Findings.Select(f => f.Code + ":" + f.Message)));

        var tribeGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeGiftOperation, Inputs(auspiceGift.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine))));
        Assert.True(tribeGift.Succeeded);

        var attributePriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAttributePrioritiesOperation, Inputs(tribeGift.Outputs,
            ("primaryCategoryId", WerewolfAttributeCategoryIdentifiers.Physical),
            ("secondaryCategoryId", WerewolfAttributeCategoryIdentifiers.Social),
            ("tertiaryCategoryId", WerewolfAttributeCategoryIdentifiers.Mental))));
        Assert.True(attributePriorities.Succeeded, "SelectAttributePriorities failed: " + string.Join("; ", attributePriorities.Findings.Select(f => f.Code + ":" + f.Message)));

        var attributeAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAttributesOperation, Inputs(attributePriorities.Outputs,
            ("attributePriorityOrder", attributePriorities.Outputs["attributePriorityOrder"]),
            ("attributeBudgets", attributePriorities.Outputs["attributeBudgets"]),
            ("attributes", "character.attribute.strength:4,character.attribute.dexterity:3,character.attribute.stamina:3,character.attribute.charisma:3,character.attribute.manipulation:3,character.attribute.appearance:2,character.attribute.perception:2,character.attribute.intelligence:2,character.attribute.wits:2"))));
        Assert.True(attributeAllocation.Succeeded, "AllocateAttributes failed: " + string.Join("; ", attributeAllocation.Findings.Select(f => f.Code + ":" + f.Message)));

        var abilityPriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAbilityPrioritiesOperation, Inputs(attributeAllocation.Outputs,
            ("primaryCategoryId", WerewolfAbilityCategoryIdentifiers.Talents),
            ("secondaryCategoryId", WerewolfAbilityCategoryIdentifiers.Skills),
            ("tertiaryCategoryId", WerewolfAbilityCategoryIdentifiers.Knowledges))));
        Assert.True(abilityPriorities.Succeeded, "SelectAbilityPriorities failed: " + string.Join("; ", abilityPriorities.Findings.Select(f => f.Code + ":" + f.Message)));

        var abilityAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAbilitiesOperation, Inputs(abilityPriorities.Outputs,
            ("abilityPriorityOrder", abilityPriorities.Outputs["abilityPriorityOrder"]),
            ("abilityBudgets", abilityPriorities.Outputs["abilityBudgets"]),
            ("abilities", "character.ability.alertness:2,character.ability.athletics:2,character.ability.brawl:2,character.ability.empathy:2,character.ability.expression:2,character.ability.intimidation:2,character.ability.subterfuge:1,character.ability.stealth:1,character.ability.survival:0,character.ability.computer:1,character.ability.drive:2,character.ability.etiquette:2,character.ability.law:1,character.ability.leadership:2,character.ability.occult:1,character.ability.performance:2,character.ability.politics:1,character.ability.investigation:1"))));
        Assert.True(abilityAllocation.Succeeded, "AllocateAbilities failed: " + string.Join("; ", abilityAllocation.Findings.Select(f => f.Code + ":" + f.Message)));

        var backgrounds = registry.Execute(Request(WerewolfReferenceRuntime.AllocateBackgroundsOperation, Inputs(abilityAllocation.Outputs,
            ("backgrounds", "character.background.allies:2,character.background.contacts:1,character.background.mentor:0,character.background.resources:1,character.background.rites:1"))));
        Assert.True(backgrounds.Succeeded, "AllocateBackgrounds failed: " + string.Join("; ", backgrounds.Findings.Select(f => f.Code + ":" + f.Message)));

        var resources = registry.Execute(Request(WerewolfReferenceRuntime.InitializeResourcesAndRankOperation, Inputs(backgrounds.Outputs)));
        Assert.True(resources.Succeeded, "InitializeResourcesAndRank failed: " + string.Join("; ", resources.Findings.Select(f => f.Code + ":" + f.Message)));

        var ragbashRenown = registry.Execute(Request(WerewolfReferenceRuntime.SelectRagabashRenownOperation, Inputs(resources.Outputs,
            ("glory", "0"),
            ("honor", "0"),
            ("wisdom", "3"))));
        Assert.True(ragbashRenown.Succeeded, "SelectRagabashRenown failed: " + string.Join("; ", ragbashRenown.Findings.Select(f => f.Code + ":" + f.Message)));

        var identity = registry.Execute(Request(WerewolfReferenceRuntime.SetIdentityNameOperation, Inputs(ragbashRenown.Outputs, ("identityName", "test-character"))));
        Assert.True(identity.Succeeded, "SetIdentityName failed: " + string.Join("; ", identity.Findings.Select(f => f.Code + ":" + f.Message)));

        var completed = registry.Execute(Request(WerewolfReferenceRuntime.CompleteCharacterOperation, Inputs(identity.Outputs)));
        Assert.True(completed.Succeeded, string.Join("; ", completed.Findings.Select(f => f.Code + ":" + f.Message)));
        Assert.Equal("Completed", completed.Outputs["status"]);
        Assert.NotNull(completed.Outputs["snapshot"]);
    }

    [Fact]
    public void RuntimeRegistryRejectsAlreadyCompletedDraft()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        Assert.True(created.Succeeded, "CreateCharacter failed: " + string.Join("; ", created.Findings.Select(f => f.Code + ":" + f.Message)));
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, Inputs(created.Outputs, ("raceId", WerewolfRaceIdentifiers.Homid))));
        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, Inputs(race.Outputs, ("auspiceId", WerewolfAuspiceIdentifiers.Ragabash))));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, Inputs(auspice.Outputs, ("tribeId", WerewolfTribeIdentifiers.GlassWalkers))));
        var raceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceGiftOperation, Inputs(tribe.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.HomidMasterOfFire))));
        var auspiceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceGiftOperation, Inputs(raceGift.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.RagabashOpenSeal))));
        var tribeGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeGiftOperation, Inputs(auspiceGift.Outputs, ("giftId", WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine))));
        var attributePriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAttributePrioritiesOperation, Inputs(tribeGift.Outputs,
            ("primaryCategoryId", WerewolfAttributeCategoryIdentifiers.Physical),
            ("secondaryCategoryId", WerewolfAttributeCategoryIdentifiers.Social),
            ("tertiaryCategoryId", WerewolfAttributeCategoryIdentifiers.Mental))));
        var attributeAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAttributesOperation, Inputs(attributePriorities.Outputs,
            ("attributePriorityOrder", attributePriorities.Outputs["attributePriorityOrder"]),
            ("attributeBudgets", attributePriorities.Outputs["attributeBudgets"]),
            ("attributes", "character.attribute.strength:4,character.attribute.dexterity:3,character.attribute.stamina:3,character.attribute.charisma:3,character.attribute.manipulation:3,character.attribute.appearance:2,character.attribute.perception:2,character.attribute.intelligence:2,character.attribute.wits:2"))));
        Assert.True(attributeAllocation.Succeeded, "AllocateAttributes failed: " + string.Join("; ", attributeAllocation.Findings.Select(f => f.Code + ":" + f.Message)));
        var abilityPriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAbilityPrioritiesOperation, Inputs(attributeAllocation.Outputs,
            ("primaryCategoryId", WerewolfAbilityCategoryIdentifiers.Talents),
            ("secondaryCategoryId", WerewolfAbilityCategoryIdentifiers.Skills),
            ("tertiaryCategoryId", WerewolfAbilityCategoryIdentifiers.Knowledges))));
        Assert.True(abilityPriorities.Succeeded, "SelectAbilityPriorities failed: " + string.Join("; ", abilityPriorities.Findings.Select(f => f.Code + ":" + f.Message)));

        var abilityAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAbilitiesOperation, Inputs(abilityPriorities.Outputs,
            ("abilityPriorityOrder", abilityPriorities.Outputs["abilityPriorityOrder"]),
            ("abilityBudgets", abilityPriorities.Outputs["abilityBudgets"]),
            ("abilities", "character.ability.alertness:2,character.ability.athletics:2,character.ability.brawl:2,character.ability.empathy:2,character.ability.expression:2,character.ability.intimidation:2,character.ability.subterfuge:1,character.ability.stealth:1,character.ability.survival:0,character.ability.computer:1,character.ability.drive:2,character.ability.etiquette:2,character.ability.law:1,character.ability.leadership:2,character.ability.occult:1,character.ability.performance:2,character.ability.politics:1,character.ability.investigation:1"))));
        Assert.True(abilityAllocation.Succeeded, "AllocateAbilities failed: " + string.Join("; ", abilityAllocation.Findings.Select(f => f.Code + ":" + f.Message)));

        var backgrounds = registry.Execute(Request(WerewolfReferenceRuntime.AllocateBackgroundsOperation, Inputs(abilityAllocation.Outputs,
            ("backgrounds", "character.background.allies:2,character.background.contacts:1,character.background.mentor:0,character.background.resources:1,character.background.rites:1"))));
        Assert.True(backgrounds.Succeeded, "AllocateBackgrounds failed: " + string.Join("; ", backgrounds.Findings.Select(f => f.Code + ":" + f.Message)));

        var resources = registry.Execute(Request(WerewolfReferenceRuntime.InitializeResourcesAndRankOperation, Inputs(backgrounds.Outputs)));
        Assert.True(resources.Succeeded, "InitializeResourcesAndRank failed: " + string.Join("; ", resources.Findings.Select(f => f.Code + ":" + f.Message)));

        var ragbashRenown = registry.Execute(Request(WerewolfReferenceRuntime.SelectRagabashRenownOperation, Inputs(resources.Outputs,
            ("glory", "0"),
            ("honor", "0"),
            ("wisdom", "3"))));
        Assert.True(ragbashRenown.Succeeded, "SelectRagabashRenown failed: " + string.Join("; ", ragbashRenown.Findings.Select(f => f.Code + ":" + f.Message)));

        var identity = registry.Execute(Request(WerewolfReferenceRuntime.SetIdentityNameOperation, Inputs(ragbashRenown.Outputs, ("identityName", "test-character"))));
        Assert.True(identity.Succeeded, "SetIdentityName failed: " + string.Join("; ", identity.Findings.Select(f => f.Code + ":" + f.Message)));

        var first = registry.Execute(Request(WerewolfReferenceRuntime.CompleteCharacterOperation, Inputs(identity.Outputs)));
        Assert.True(first.Succeeded, "First completion failed: " + string.Join("; ", first.Findings.Select(f => f.Code + ":" + f.Message)));

        var second = registry.Execute(Request(WerewolfReferenceRuntime.CompleteCharacterOperation, Inputs(first.Outputs)));

        Assert.False(second.Succeeded);
        Assert.Contains(second.Findings, finding => finding.Code == "DraftAlreadyCompleted");
    }

    [Fact]
    public void RenownAbsenceBlocksCompletion()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            Renown = new Dictionary<string, int?>(StringComparer.Ordinal)
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfCharacterCompletionErrorCode.RenownNotInitialized);
    }

    [Fact]
    public void OptionalIdentityFieldsDoNotBlockCompletion()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash) with
        {
            NarrativeFields = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["character-concept"] = null,
                ["character-goals"] = null,
                ["character-relationships"] = null
            }
        };

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.True(result.Succeeded, Format(result.Findings));
    }

    [Theory]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash)]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Theurge)]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Philodox)]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Galliard)]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ahroun)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Ragabash)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Theurge)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Philodox)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Galliard)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Ahroun)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Ragabash)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Theurge)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Philodox)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Galliard)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Ahroun)]
    public void AllSupportedRaceAuspiceGlassWalkersPathsComplete(string race, string auspice)
    {
        var deformity = StringComparer.Ordinal.Equals(race, WerewolfRaceIdentifiers.Metis) ? WerewolfMetisDeformityIdentifiers.Horns : null;
        var draft = BuildCompletedDraft(race, auspice, deformity);

        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        Assert.True(result.Succeeded, Format(result.Findings, race, auspice));
        Assert.NotNull(result.Snapshot);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, result.Snapshot!.Tribe);
    }

    [Fact]
    public void PackageSourceValidationIncludesCompletionFile()
    {
        var root = Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf");
        var result = RuleSetPackageSourceValidator.Validate(root);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Findings.Select(f => $"{f.Severity}|{f.Code}|{f.Path}|{f.Message}")));
        Assert.Contains("CharacterCreation/WerewolfCharacterCompletion.cs", result.FileInventory);
    }

    private static WerewolfInitializedCharacterState BuildCompletedDraft(string race, string auspice, string? metisDeformity = null)
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-completion"), 1) with
        {
            Race = race,
            Auspice = auspice,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = metisDeformity,
            RaceGift = race switch
            {
                WerewolfRaceIdentifiers.Homid => WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
                WerewolfRaceIdentifiers.Metis => WerewolfInitialGiftIdentifiers.MetisCreateElement,
                WerewolfRaceIdentifiers.Lupus => WerewolfInitialGiftIdentifiers.LupusHareLeap,
                _ => null
            },
            AuspiceGift = auspice switch
            {
                WerewolfAuspiceIdentifiers.Ragabash => WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
                WerewolfAuspiceIdentifiers.Theurge => WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech,
                WerewolfAuspiceIdentifiers.Philodox => WerewolfInitialGiftIdentifiers.PhilodoxResistPain,
                WerewolfAuspiceIdentifiers.Galliard => WerewolfInitialGiftIdentifiers.GalliardBeastSpeech,
                WerewolfAuspiceIdentifiers.Ahroun => WerewolfInitialGiftIdentifiers.AhrounFallingTouch,
                _ => null
            },
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
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
                [WerewolfAbilityIdentifiers.Empathy] = 1,
                [WerewolfAbilityIdentifiers.Intimidation] = 1,
                [WerewolfAbilityIdentifiers.Expression] = 1,
                [WerewolfAbilityIdentifiers.Subterfuge] = 1,
                [WerewolfAbilityIdentifiers.Stealth] = 1,
                [WerewolfAbilityIdentifiers.Survival] = 1,
                [WerewolfAbilityIdentifiers.Computer] = 1,
                [WerewolfAbilityIdentifiers.Drive] = 1,
                [WerewolfAbilityIdentifiers.Etiquette] = 1,
                [WerewolfAbilityIdentifiers.Law] = 1,
                [WerewolfAbilityIdentifiers.Leadership] = 1,
                [WerewolfAbilityIdentifiers.Occult] = 1,
                [WerewolfAbilityIdentifiers.Performance] = 1,
                [WerewolfAbilityIdentifiers.Politics] = 1,
                [WerewolfAbilityIdentifiers.Investigation] = 1
            },
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 2,
                [WerewolfBackgroundIdentifiers.Contacts] = 1,
                [WerewolfBackgroundIdentifiers.Mentor] = 0,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 1
            },
            Resources = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfCharacterResourceIdentifiers.RagePermanent] = race == WerewolfRaceIdentifiers.Metis ? 3 : 1,
                [WerewolfCharacterResourceIdentifiers.RageCurrent] = race == WerewolfRaceIdentifiers.Metis ? 3 : 1,
                [WerewolfCharacterResourceIdentifiers.GnosisPermanent] = race == WerewolfRaceIdentifiers.Lupus ? 5 : race == WerewolfRaceIdentifiers.Metis ? 3 : 1,
                [WerewolfCharacterResourceIdentifiers.GnosisCurrent] = race == WerewolfRaceIdentifiers.Lupus ? 5 : race == WerewolfRaceIdentifiers.Metis ? 3 : 1,
                [WerewolfCharacterResourceIdentifiers.WillpowerPermanent] = 3,
                [WerewolfCharacterResourceIdentifiers.WillpowerCurrent] = 3
            },
            Renown = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.GloryPermanent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Galliard => 2,
                    WerewolfAuspiceIdentifiers.Ahroun => 2,
                    _ => 0
                },
                [WerewolfRenownIdentifiers.GloryCurrent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Galliard => 2,
                    WerewolfAuspiceIdentifiers.Ahroun => 2,
                    _ => 0
                },
                [WerewolfRenownIdentifiers.HonorPermanent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Ragabash => 0,
                    WerewolfAuspiceIdentifiers.Philodox => 3,
                    WerewolfAuspiceIdentifiers.Ahroun => 1,
                    _ => 0
                },
                [WerewolfRenownIdentifiers.HonorCurrent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Ragabash => 0,
                    WerewolfAuspiceIdentifiers.Philodox => 3,
                    WerewolfAuspiceIdentifiers.Ahroun => 1,
                    _ => 0
                },
                [WerewolfRenownIdentifiers.WisdomPermanent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Ragabash => 3,
                    WerewolfAuspiceIdentifiers.Theurge => 3,
                    WerewolfAuspiceIdentifiers.Galliard => 1,
                    _ => 0
                },
                [WerewolfRenownIdentifiers.WisdomCurrent] = auspice switch
                {
                    WerewolfAuspiceIdentifiers.Ragabash => 3,
                    WerewolfAuspiceIdentifiers.Theurge => 3,
                    WerewolfAuspiceIdentifiers.Galliard => 1,
                    _ => 0
                }
            },
            Rank = WerewolfRankIdentifiers.Cliath,
            RankValue = 1,
            IdentityName = "test-character",
            RequiredNextSteps = Array.AsReadOnly<string>([]),
            DisabledCapabilities = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["additional-gift-purchase"] = "disabled",
                ["runtime-gift-execution"] = "disabled"
            }
        };

        return draft;
    }

    private static string ComputeSnapshotFingerprint(WerewolfCharacterSnapshot snapshot)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { SkipValidation = true });
        writer.WriteStartObject();
        writer.WriteNumber("draftVersion", snapshot.DraftVersion);
        writer.WriteString("status", snapshot.Status.ToString());
        writer.WriteString("race", snapshot.Race);
        writer.WriteString("auspice", snapshot.Auspice);
        writer.WriteString("tribe", snapshot.Tribe);
        writer.WriteString("identityName", snapshot.IdentityName ?? string.Empty);
        writer.WriteNumber("rankValue", snapshot.RankValue ?? 0);
        writer.WriteEndObject();
        writer.Flush();
        return Convert.ToBase64String(ms.ToArray());
    }

    private static string Format(IEnumerable<WerewolfCharacterCompletionFinding> findings, string? race = null, string? auspice = null)
    {
        var prefix = string.IsNullOrEmpty(race) ? string.Empty : $"[{race}/{auspice}] ";
        return prefix + string.Join("; ", findings.Select(f => $"{f.Code}:{f.Message}"));
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
            return new WerewolfCharacterDraftIdentity("runtime-draft-completion");
        }
    }
}
