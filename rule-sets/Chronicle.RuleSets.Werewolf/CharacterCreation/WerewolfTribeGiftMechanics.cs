using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

/// <summary>
/// One resolved Tribe Gift activation: the typed modifier it produces plus
/// the source-derived numbers behind it.
/// </summary>
/// <remarks>
/// The record carries its own day-duration capability rather than borrowing a
/// shared one. The source expresses some durations in days ("Dura um dia por
/// sucesso", source line 2133) and defines no turn-to-day conversion anywhere
/// (line 2950 defines a turn only inside combat), so encoding days as turns
/// would invent a constant. <see cref="DaysPerSuccess"/> therefore carries the
/// source's own day count, and <see cref="DurationTurns"/> stays persistent so
/// no false per-turn value is produced.
/// </remarks>
public sealed record WerewolfTribeGiftMechanic(
    string GiftKey,
    WerewolfActiveGiftEffectKind Kind,
    int Magnitude,
    int DurationTurns,
    object? Payload,
    string SourceLocator)
{
    /// <summary>
    /// Days the effect lasts per success, when the source states its duration
    /// in days. Null whenever the source states no day-based duration.
    /// </summary>
    public int? DaysPerSuccess { get; init; }
}

// ---------------------------------------------------------------------------
// Typed payloads. Every field traces to the cited source line.
// ---------------------------------------------------------------------------

/// <summary>
/// Source line 2109: 1 Willpower, Manipulation + Crafts at difficulty 7, and
/// the outcome is binary - the machine spirits obey or they do not. No
/// individual machine spirit is modelled, because the source describes none.
/// </summary>
public sealed record WerewolfSimpleMachineCommandPayload(
    int TestDifficulty,
    string TestAttribute,
    string TestAbility,
    int WillpowerCost,
    bool IsCommanded,
    IReadOnlyList<string> MachineKinds);

/// <summary>
/// Source line 2115: the character's permanent Glory rating is added to the dice
/// pool in indirect shooting maneuvers only, it causes no direct damage, and
/// the effect is permanent.
/// </summary>
/// <remarks>
/// <see cref="CausesNoDirectDamage"/> is true because line 2115 says
/// "manobras de tiro indiretas (nao causa dano direto)": the parentheses deny
/// direct damage. An inverted value here would encode the opposite of the
/// source while the prose above said otherwise.
/// </remarks>
public sealed record WerewolfTrickShotPayload(
    int PermanentGloryDiceBonus,
    bool IndirectManeuversOnly,
    bool CausesNoDirectDamage);

/// <summary>
/// Source line 2133: 1 Gnosis, Charisma + Performance at difficulty 8, one day
/// of duration per success, and explicitly no duplication of Attributes.
/// </summary>
public sealed record WerewolfDoppelgangerPayload(
    int TestDifficulty,
    string TestAttribute,
    string TestAbility,
    int GnosisCost,
    int DaysPerSuccess,
    bool DuplicatesAttributes,
    IReadOnlyList<string> MimickedCreatureClasses);

/// <summary>
/// Source line 2143: a permanent effect that lets Rage and Gnosis be used in
/// the same turn without penalty. This is the named "Dons especificos"
/// exception to the general prohibition at source lines 1677 and 1691.
/// </summary>
/// <remarks>
/// Recorded ambiguity: line 2143 lifts the same-turn restriction "sem
/// penalidade" (without penalty), while the overridden rules at lines 1677
/// ("Nao se pode usar Furia e Gnose no mesmo turno") and 1691 ("Nao podem ser
/// usadas no mesmo turno (exceto Dons especificos)") state a flat prohibition,
/// not a penalty. A penalty and a prohibition are not the same rule, so the two
/// texts cannot both be read literally: if 1677/1691 forbid outright, there is
/// no penalty left for 2143 to waive, and if they only penalised, 2143's
/// "sem penalidade" would be redundant. Chronicle resolves this the only way
/// that does not invent a rule: it suppresses the prohibition for this Gift,
/// citing both lines, and leaves the discrepancy recorded rather than resolved
/// silently. See <see cref="NoPenaltyWordingAmbiguity"/>.
/// </remarks>
public sealed record WerewolfChaosMechanicsPayload(
    bool AllowsRageAndGnosisSameTurn,
    bool AllowsInstantFetishGiftAndUmbralActivation,
    string OverridesRule)
{
    /// <summary>
    /// The unresolved conflict between line 2143's "sem penalidade" wording and
    /// the penalty-free prohibition of lines 1677 and 1691. It is carried on the
    /// payload so no reader of the mechanic can mistake the suppression of the
    /// prohibition for a source certainty.
    /// </summary>
    public string NoPenaltyWordingAmbiguity { get; init; } = string.Empty;
}

/// <summary>
/// Why a Tribe Gift cannot produce a typed outcome yet. Unlike a blocked
/// Breed or Auspice Gift, this payload also carries the deferral note of a
/// Gift that no wave has source-audited yet, so the reason distinguishes a
/// missing dependency from simple deferral.
/// </summary>
public sealed record WerewolfTribeGiftBlockedPayload(
    string Reason,
    string MissingSubsystem,
    string SourceLocator);

/// <summary>
/// Explicit completion status for a Tribe Gift.
/// </summary>
/// <remarks>
/// This is deliberately a Tribe-local enum. The shared
/// <see cref="WerewolfBreedGiftStatus"/> is asserted by the Breed and Auspice
/// guards against exactly two members, so it must not grow a third; and
/// <see cref="WerewolfGiftDurationType"/> must not grow a <c>Day</c> member
/// because both duration engines resolve an unmapped member to zero turns.
/// </remarks>
public enum WerewolfTribeGiftStatus
{
    /// <summary>Produces a real, source-derived typed mechanic at runtime.</summary>
    Executable,

    /// <summary>Cannot be implemented without a subsystem the source does not define.</summary>
    Blocked,

    /// <summary>
    /// Not yet source-audited and not implemented. It says nothing about the
    /// Gift's source semantics, because nothing has established them.
    /// </summary>
    Deferred
}

public sealed record WerewolfTribeGiftAudit(
    string GiftKey,
    string NameEn,
    string NamePtBr,
    int Level,
    string OwnerKey,
    string SourceLocator,
    WerewolfTribeGiftStatus Status,
    string? Reason,
    string? MissingDependency);

/// <summary>
/// Source-derived Tribe Gift mechanics. The first wave covers source section
/// 27 (lines 2104-2143, Glass Walkers / Andarilhos do Asfalto): four Gifts the
/// source determines completely, seven it leaves blocked by a named missing
/// dependency, and 121 that no wave has source-audited yet.
/// </summary>
/// <remarks>
/// Scope and runtime reachability, stated plainly. This wave source-audited 11
/// of the 132 Tribe Gifts. The remaining 121 are classified
/// <see cref="WerewolfTribeGiftStatus.Deferred"/>, which is a statement about
/// this table's coverage and nothing more. They are deliberately NOT routed at
/// runtime: routing them all through <c>Kind = Custom</c> would overwrite nine
/// already-validated behaviours with real source semantics. Their deferred
/// status is therefore reported by <see cref="Audit"/> and
/// <see cref="StatusOf"/>, which have no production call sites, and by nothing
/// in the runtime path. The Deferred arm of <see cref="Resolve"/> is likewise
/// unreachable in production; it exists so the table itself is total.
/// </remarks>
public static class WerewolfTribeGiftMechanics
{
    private const int InstantTurns = 0;
    private const int SceneTurns = -1;
    private const int PersistentTurns = -1;

    private static readonly IReadOnlyList<string> SimpleMachineKinds =
        Array.AsReadOnly(new[] { "lever", "door", "pulley" });

    private static readonly IReadOnlyList<string> DoppelgangerCreatureClasses =
        Array.AsReadOnly(new[] { "human", "wolf", "Garou" });

    /// <summary>
    /// Every Tribe Gift key, derived from the catalog's Tribe category so the
    /// table cannot drift from the catalogued source Gifts.
    /// </summary>
    public static IReadOnlyList<string> TribeGiftKeys { get; } = new ReadOnlyCollection<string>(
        WerewolfGiftCatalog.AllDefinitions
            .Where(definition => definition.Category == WerewolfGiftCategory.Tribe)
            .Select(definition => definition.GiftKey)
            .ToList());

    /// <summary>
    /// Tribe Gifts whose complete effect the source determines and that
    /// therefore resolve to a real typed mechanic.
    /// </summary>
    public static IReadOnlyList<string> ExecutableGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Glass Walkers, source section 27 (lines 2104-2143).
        WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine,
        WerewolfGiftIdentifiers.GlassWalkersTrickShot,
        WerewolfGiftIdentifiers.GlassWalkersDoppelganger,
        WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos
    });

    /// <summary>
    /// Tribe Gifts blocked by a dependency the source does not itself define.
    /// Each entry names that dependency; none of them is blocked merely by
    /// implementation order.
    /// </summary>
    public static IReadOnlyList<string> BlockedGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Line 2112: "Reduz o tempo de reparo pela metade e subtrai os sucessos
        // da Percepção + Ofícios do total necessário para o conserto." The
        // difficulty of the test, the "total necessary for the repair" it is
        // subtracted from, and the unit of repair time are all undefined.
        WerewolfGiftIdentifiers.GlassWalkersDiagnostics,
        // Line 2119: a Perception + Science test with no stated difficulty, and
        // a cost of 1 Gnosis per sense that no scalar amount can express.
        WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos,
        // Line 2122: the affected area scales with successes ("one room" for 1,
        // "an entire neighbourhood" for 5) but never carries a numeric scale,
        // successes 2 to 4 are undefined, and no duration is stated.
        WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia,
        // Line 2126: the difficulty is "based on the complexity, generally 8",
        // a hedged default with no device-complexity scale, and the ability is
        // written "Ciencia/Computador", which names two traits at once.
        WerewolfGiftIdentifiers.GlassWalkersControleDeMaquinasComplexas,
        // Line 2129: Charisma + Guile opposed by an urban elemental's Gnosis.
        WerewolfGiftIdentifiers.GlassWalkersFavorDoElemental,
        // Line 2136: a Perception + Manha test with no difficulty, no duration,
        // and a critical failure whose consequence has no mechanical value.
        WerewolfGiftIdentifiers.GlassWalkersHarmonia,
        // Line 2140: a Charisma + Computer test at difficulty 8 plus a Web
        // Spider that the source defines nowhere outside this Gift's header.
        WerewolfGiftIdentifiers.GlassWalkersInvocarAranhaDeRede
    });

    /// <summary>
    /// Tribe Gifts that are neither executable in this wave nor blocked by a
    /// dependency this wave established. They have not been source-audited, so
    /// their deferral says nothing about their semantics.
    /// </summary>
    public static IReadOnlyList<string> DeferredGiftKeys { get; } = new ReadOnlyCollection<string>(
        TribeGiftKeys
            .Where(key => !ExecutableGiftKeys.Contains(key, StringComparer.Ordinal))
            .Where(key => !BlockedGiftKeys.Contains(key, StringComparer.Ordinal))
            .ToList());

    public static bool IsTribeGift(string giftKey) =>
        TribeGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static bool IsExecutable(string giftKey) =>
        ExecutableGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static bool IsBlocked(string giftKey) =>
        BlockedGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static bool IsDeferred(string giftKey) =>
        !IsExecutable(giftKey) && !IsBlocked(giftKey) && IsTribeGift(giftKey);

    /// <summary>
    /// The explicit status of a Tribe Gift, with the reason it is not yet
    /// executable. A Gift that is neither executable nor listed as blocked is
    /// deferred, never silently catalog-only. This is a reporting projection
    /// over the table: it has no production call site.
    /// </summary>
    public static (WerewolfTribeGiftStatus Status, string? Reason) StatusOf(string giftKey)
    {
        if (!IsTribeGift(giftKey))
        {
            throw new ArgumentException($"'{giftKey}' is not a Tribe Gift.", nameof(giftKey));
        }

        if (IsExecutable(giftKey))
        {
            return (WerewolfTribeGiftStatus.Executable, null);
        }

        return IsBlocked(giftKey)
            ? (WerewolfTribeGiftStatus.Blocked, BlockedSubsystem(giftKey))
            : (WerewolfTribeGiftStatus.Deferred, DeferredReason(giftKey));
    }

    /// <summary>
    /// Audits every Tribe Gift against the catalog, returning its explicit
    /// status. Used to prove the domain has no catalog-only remainder. This is
    /// a reporting projection over the table; it has no production call site.
    /// </summary>
    public static IReadOnlyList<WerewolfTribeGiftAudit> Audit()
    {
        var audit = new List<WerewolfTribeGiftAudit>(TribeGiftKeys.Count);

        foreach (var giftKey in TribeGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            var (status, reason) = StatusOf(giftKey);

            audit.Add(new WerewolfTribeGiftAudit(
                giftKey,
                definition?.NameEn ?? string.Empty,
                definition?.NamePtBr ?? string.Empty,
                definition?.Level ?? 0,
                definition?.OwnerKey ?? string.Empty,
                definition?.SourceLocator ?? string.Empty,
                status,
                reason,
                status == WerewolfTribeGiftStatus.Blocked ? reason : null));
        }

        return audit.AsReadOnly();
    }

    /// <summary>
    /// Resolves the typed mechanic for a Tribe Gift activation. Every
    /// registered Tribe key returns a real mechanic: a typed one when the
    /// source determines the effect, otherwise a Custom effect carrying the
    /// honest reason the Gift cannot yet produce a numeric outcome.
    /// </summary>
    /// <remarks>
    /// Runtime reachability, stated plainly: the only production call site
    /// (WerewolfGiftEffectService.ApplyEffect) calls this method only for
    /// executable or blocked Gifts. The branch below therefore serves the 121
    /// deferred Gifts for table totality and for tests only; it never runs in
    /// production, and nothing at runtime reports a deferred Gift's status.
    /// </remarks>
    public static WerewolfTribeGiftMechanic? Resolve(
        string giftKey,
        int successes,
        string currentForm,
        IReadOnlyDictionary<string, int> sheet,
        int permanentGlory = 0)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);
        var source = definition?.SourceLocator ?? "Line 0";
        var s = Math.Max(0, successes);

        if (definition is null || definition.Category != WerewolfGiftCategory.Tribe)
        {
            return null;
        }

        if (!IsExecutable(giftKey))
        {
            var (status, reason) = StatusOf(giftKey);
            return new WerewolfTribeGiftMechanic(
                giftKey,
                WerewolfActiveGiftEffectKind.Custom,
                InstantTurns,
                InstantTurns,
                new WerewolfTribeGiftBlockedPayload(
                    status == WerewolfTribeGiftStatus.Blocked ? BlockedReason(giftKey) : DeferredReason(giftKey),
                    reason ?? string.Empty,
                    source),
                source);
        }

        return giftKey switch
        {
            // Line 2109: costs 1 Willpower and requires Manipulation + Crafts at
            // difficulty 7; lasts until the end of the scene. The spirits of
            // levers, doors and pulleys either obey or they do not, so the
            // outcome is the binary "commanded" flag, exactly as the source
            // states it. No machine-spirit entity is instantiated because the
            // source describes none.
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine =>
                new(giftKey, WerewolfActiveGiftEffectKind.MachineControl, s > 0 ? 1 : 0, SceneTurns,
                    new WerewolfSimpleMachineCommandPayload(7, "Manipulation", "Crafts", 1, s > 0, SimpleMachineKinds),
                    source),

            // Line 2115: the permanent Glory rating is added to the dice pool,
            // but only in indirect shooting maneuvers, and it deals no direct
            // damage. WerewolfActiveGiftEffectKind.TestBonus would apply the
            // bonus to every test, and CombatDamageBonus would invent damage
            // the source explicitly denies, so the restriction stays in the
            // payload rather than being overstated by a shared kind. The last
            // payload argument is true because "nao causa dano direto" denies
            // direct damage; false would state the opposite of the source.
            WerewolfGiftIdentifiers.GlassWalkersTrickShot =>
                new(giftKey, WerewolfActiveGiftEffectKind.Custom, Math.Max(0, permanentGlory), PersistentTurns,
                    new WerewolfTrickShotPayload(Math.Max(0, permanentGlory), true, true),
                    source),

            // Line 2133: 1 Gnosis, Charisma + Performance at difficulty 8, one
            // day per success, and it does not duplicate Attributes. The
            // duration is days, which the shared turn counters cannot express,
            // so DaysPerSuccess carries the source's own count. It mimics
            // appearance, not form, so FormTransformation would misstate it.
            WerewolfGiftIdentifiers.GlassWalkersDoppelganger =>
                new(giftKey, WerewolfActiveGiftEffectKind.Custom, s, PersistentTurns,
                    new WerewolfDoppelgangerPayload(8, "Charisma", "Performance", 1, 1, false, DoppelgangerCreatureClasses),
                    source)
                { DaysPerSuccess = 1 },

            // Line 2143: permanent; Rage and Gnosis in the same turn without
            // penalty. No shared kind expresses a resource-adjacency
            // exemption, so the whole outcome is in the payload.
            WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos =>
                new(giftKey, WerewolfActiveGiftEffectKind.Custom, 1, PersistentTurns,
                    new WerewolfChaosMechanicsPayload(
                        true,
                        true,
                        "Source lines 1677 and 1691 forbid using Rage and Gnosis on the same turn except for named specific Gifts; line 2143 names this one.")
                    {
                        NoPenaltyWordingAmbiguity =
                            "Line 2143 waives a penalty ('sem penalidade'), but lines 1677 and 1691 state a " +
                            "flat prohibition rather than a penalty. Chronicle suppresses the prohibition for " +
                            "this Gift and records the conflict instead of resolving it silently."
                    },
                    source),

            _ => null
        };
    }

    private static string BlockedReason(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.GlassWalkersDiagnostics =>
            "Source line 2112 requires a Perception + Crafts test to find the fault and halves the " +
            "repair time while subtracting successes from the total necessary for the repair. The " +
            "source states no difficulty for that test, never defines the total the successes are " +
            "subtracted from, and uses no unit anywhere for repair time, so the effect has no " +
            "computable numeric result.",
        WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos =>
            "Source line 2119 requires a Perception + Science test but states no difficulty, so the " +
            "test cannot be rolled against a source-defined number. Its cost is 1 Gnosis point per " +
            "sense, which a single scalar cost amount cannot express.",
        WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia =>
            "Source line 2122 scales the affected area with successes, from one room at 1 success to " +
            "an entire neighbourhood at 5, but attaches no numeric scale to that area, leaves " +
            "successes 2 to 4 undefined, and states no duration for the blackout.",
        WerewolfGiftIdentifiers.GlassWalkersControleDeMaquinasComplexas =>
            "Source line 2126 sets the difficulty by the complexity of the device and offers 8 only as " +
            "a general default, with no scale of device complexity to resolve it against. The test's " +
            "ability is written 'Ciencia/Computador', naming two traits at once, so the pool it " +
            "contributes is ambiguous.",
        WerewolfGiftIdentifiers.GlassWalkersFavorDoElemental =>
            "Source line 2129 opposes a Charisma + Guile test against an urban elemental's Gnosis. No " +
            "urban-elemental entity carrying a Gnosis rating exists to be addressed and opposed, so " +
            "the resisted test has no second side.",
        WerewolfGiftIdentifiers.GlassWalkersHarmonia =>
            "Source line 2136 requires a Perception + Manha test with no stated difficulty and gives " +
            "the effect no duration. Its critical-failure consequence is dangerous disinformation " +
            "from mischievous spirits, with no mechanical value attached.",
        WerewolfGiftIdentifiers.GlassWalkersInvocarAranhaDeRede =>
            "Source line 2140 summons a Web Spider and halves all computer-related difficulties. The " +
            "term 'Aranha de Rede' appears exactly once in the whole source, in this Gift's own " +
            "header, so the summoned entity has no source statistics, and the source never " +
            "classifies which difficulties count as computer-related.",
        _ => "Blocked: the source leaves a dependency of this Gift undefined."
    };

    private static string BlockedSubsystem(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.GlassWalkersDiagnostics =>
            "A repair-progress model: a numeric 'total necessary for the repair' to subtract " +
            "Perception + Crafts successes from, plus a unit for repair time so halving it is " +
            "computable.",
        WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos =>
            "A per-sense cost model: a Gnosis cost that multiplies by the number of senses replaced, " +
            "which a single scalar CostAmount cannot represent.",
        WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia =>
            "An area-scale model: a numeric scale that turns a room or a neighbourhood into a " +
            "magnitude, with defined values for successes 2 to 4, plus a stated duration.",
        WerewolfGiftIdentifiers.GlassWalkersControleDeMaquinasComplexas =>
            "A device-complexity scale that resolves the difficulty of line 2126 for a given device, " +
            "and a resolution of the slash-ambiguous 'Ciencia/Computador' ability.",
        WerewolfGiftIdentifiers.GlassWalkersFavorDoElemental =>
            "An addressable urban-elemental entity exposing a Gnosis rating, so a resisted test can " +
            "name the pool it is opposed against.",
        WerewolfGiftIdentifiers.GlassWalkersHarmonia =>
            "A stated difficulty and duration for the Perception + Manha test, and a mechanical value " +
            "for the critical-failure disinformation.",
        WerewolfGiftIdentifiers.GlassWalkersInvocarAranhaDeRede =>
            "Source statistics for the Web Spider, and a classification of which difficulties the " +
            "halving applies to.",
        _ => "unknown"
    };

    /// <summary>
    /// Why a deferred Tribe Gift is not implemented yet. It deliberately says
    /// nothing about the Gift's source semantics: only the 11 Glass Walkers
    /// Gifts were source-audited, and 121 Gifts were not, so any claim that
    /// their semantics are settled would be unsupported.
    /// </summary>
    private static string DeferredReason(string giftKey)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);
        var alias = definition?.AliasedGiftKey;

        return alias is null
            ? "Not yet source-audited or implemented; pending a later Tribe Gift wave."
            : "Not yet source-audited or implemented; pending a later Tribe Gift wave. " +
              $"The catalog records an alias to '{alias}', but this Gift has not been source-audited " +
              "in this wave and no mechanic routes the alias yet.";
    }
}