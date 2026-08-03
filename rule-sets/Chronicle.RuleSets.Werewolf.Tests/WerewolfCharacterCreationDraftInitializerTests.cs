using System.Collections.ObjectModel;
using System.Globalization;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCharacterCreationDraftInitializerTests
{
    [Fact]
    public void InitializesCharacterCreationDraft()
    {
        var result = Initializer("draft-001").Initialize(new WerewolfCreateCharacterRequest("request-001"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal("draft-001", result.Draft.DraftIdentity.Value);
        Assert.Equal(WerewolfCharacterDraftStatus.Initialized, result.Draft.Status);
        Assert.Equal(1, result.Draft.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == "DraftInitialized");
    }

    [Fact]
    public void UsesInjectedChronicleIdentityAbstraction()
    {
        var identitySource = new TestIdentitySource("chronicle-owned-id");

        var result = new WerewolfCharacterCreationDraftInitializer(identitySource).Initialize(new WerewolfCreateCharacterRequest("request-001"));

        Assert.True(identitySource.WasCalled);
        Assert.Equal("request-001", identitySource.LastRequestId);
        Assert.Equal("chronicle-owned-id", result.Draft?.DraftIdentity.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingOrMalformedRequestIsRejected(string requestId)
    {
        var result = Initializer("unused").Initialize(new WerewolfCreateCharacterRequest(requestId));

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
        Assert.Contains(result.Findings, finding => finding.Code == "MissingRequestId");
    }

    [Fact]
    public void InitialStateIsDeterministic()
    {
        var first = Initializer("draft-001").Initialize(new WerewolfCreateCharacterRequest("request-001"));
        var second = Initializer("draft-001").Initialize(new WerewolfCreateCharacterRequest("request-001"));

        Assert.Equal(Format(first), Format(second));
    }

    [Fact]
    public void DoesNotAutomaticallySelectRaceAuspiceOrTribe()
    {
        var draft = RequiredDraft();

        Assert.Null(draft.Race);
        Assert.Null(draft.Auspice);
        Assert.Null(draft.Tribe);
        Assert.Null(draft.MetisDeformity);
    }

    [Fact]
    public void DisabledCapabilitiesArePreserved()
    {
        var draft = RequiredDraft();

        Assert.Equal("disabled", draft.DisabledCapabilities["additional-gift-purchase"]);
        Assert.Equal("disabled", draft.DisabledCapabilities["runtime-gift-execution"]);
        Assert.Empty(draft.Gifts);
    }

    [Fact]
    public void ReturnedCollectionsAreImmutableSnapshots()
    {
        var draft = RequiredDraft();

        Assert.IsType<ReadOnlyDictionary<string, int?>>(draft.Attributes);
        Assert.IsType<ReadOnlyDictionary<string, int?>>(draft.Abilities);
        Assert.IsType<ReadOnlyDictionary<string, int?>>(draft.Backgrounds);
        Assert.IsType<ReadOnlyDictionary<string, int?>>(draft.Resources);
        Assert.IsType<ReadOnlyDictionary<string, string?>>(draft.NarrativeFields);
        Assert.NotSame(draft.Attributes, RequiredDraft().Attributes);
    }

    [Fact]
    public void RepeatedRequestsDoNotShareMutableState()
    {
        var initializer = new WerewolfCharacterCreationDraftInitializer(new SequenceIdentitySource());

        var first = initializer.Initialize(new WerewolfCreateCharacterRequest("request-001"));
        var second = initializer.Initialize(new WerewolfCreateCharacterRequest("request-002"));

        Assert.NotEqual(first.Draft?.DraftIdentity, second.Draft?.DraftIdentity);
        Assert.NotSame(first.Draft?.Attributes, second.Draft?.Attributes);
        Assert.NotSame(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
    }

    [Fact]
    public void InitializerHasNoForbiddenIntegrationDependencies()
    {
        var files = Directory.EnumerateFiles(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText);
        var forbidden = new[]
        {
            "Chronicle.Persistence",
            "Chronicle.Presentation",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Microsoft.EntityFrameworkCore",
            "OpenAI",
            "HttpClient",
            "DbContext",
            "File.",
            "Directory.",
            "Random",
            "Campaign"
        };

        foreach (var token in forbidden)
        {
            Assert.DoesNotContain(files, text => text.Contains(token, StringComparison.Ordinal));
        }
    }

    private static WerewolfInitializedCharacterState RequiredDraft()
    {
        var result = Initializer("draft-001").Initialize(new WerewolfCreateCharacterRequest("request-001"));
        return result.Draft ?? throw new InvalidOperationException("Expected initialized draft.");
    }

    private static WerewolfCharacterCreationDraftInitializer Initializer(string identity)
    {
        return new WerewolfCharacterCreationDraftInitializer(new TestIdentitySource(identity));
    }

    private static string Format(WerewolfCreateCharacterResultPayload payload)
    {
        var draft = payload.Draft;
        return string.Join(
            "|",
            payload.Succeeded,
            draft?.DraftIdentity.Value,
            draft?.Status,
            draft?.DraftVersion,
            draft?.Race ?? "<unset>",
            draft?.Auspice ?? "<unset>",
            draft?.Tribe ?? "<unset>",
            draft?.MetisDeformity ?? "<unset>",
            string.Join(",", draft?.Attributes.Select(entry => $"{entry.Key}:{entry.Value?.ToString(CultureInfo.InvariantCulture) ?? "<unset>"}") ?? []),
            string.Join(",", draft?.RequiredNextSteps ?? []),
            string.Join(",", payload.Findings.Select(finding => finding.Code)));
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

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }

    private sealed class TestIdentitySource(string identity) : IWerewolfCharacterDraftIdentitySource
    {
        public bool WasCalled { get; private set; }

        public string? LastRequestId { get; private set; }

        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            WasCalled = true;
            LastRequestId = request.RequestId;
            return new WerewolfCharacterDraftIdentity(identity);
        }
    }

    private sealed class SequenceIdentitySource : IWerewolfCharacterDraftIdentitySource
    {
        private int next;

        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            next++;
            return new WerewolfCharacterDraftIdentity($"draft-{next}");
        }
    }
}
