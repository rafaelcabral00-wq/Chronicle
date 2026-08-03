---
id: ADR-0040
title: English-First Technical Language, Localization Boundaries, and Content Language Policy
status: Proposed
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Product Architecture
supersedes:
  - ADR-0040@0.1.0
superseded_by: null
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
  - RFC-0003
  - RFC-0004
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0008
  - RFC-0009
  - RFC-0010
  - RFC-0011
  - RFC-0012
  - RFC-0013
  - RFC-0014
  - RFC-0015
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0020
  - RFC-0021
  - RFC-0022
  - RFC-0023
  - RFC-0024
  - RFC-0025
  - RFC-0026
  - RFC-0027
  - RFC-0028
  - RFC-0029
  - RFC-0030
  - RFC-0031
  - RFC-0032
  - RFC-0033
  - RFC-0034
  - ADR-0001
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0007
  - ADR-0008
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0019
  - ADR-0020
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0027
  - ADR-0028
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0039
  - ADR-0041
  - ADR-0042
  - ADR-0043
implements: []
related_to:
  - ADR-0044
---

> **"Chronicle may tell stories in any language. Its technical truth must have one stable language."**

# English-First Technical Language, Localization Boundaries, and Content Language Policy

## 1. Status

**Proposed**

This ADR defines Chronicle's language and localization architecture.

The decision is:

- English is the canonical language for:
  - source code;
  - project and folder names;
  - namespaces;
  - public and internal technical contracts;
  - schema identifiers;
  - database table and column names;
  - state keys;
  - event keys;
  - error keys;
  - logging keys;
  - migration names;
  - package identifiers;
  - configuration keys;
  - architecture documentation;
  - contributor documentation;
  - base UI resources;
  - installer source text;
- user-facing UI is localization-ready from the MVP;
- English is the base and fallback locale;
- generated narrative language is a Campaign or user preference and is not forced to English;
- Rule Set packages may provide localized display resources without changing canonical technical identifiers;
- localization is performed through stable semantic resource keys;
- localized strings are never used as persistence values, routing keys, event types, state identifiers, or contract discriminators;
- locale selection is separate from narrative-generation language;
- persisted user-authored and generated content keeps its original language;
- Chronicle does not translate existing Campaign history automatically;
- localization support is implemented in the MVP foundation so adding more languages does not require replacing UI, contracts, or persistence;
- right-to-left and pluralization support must not be structurally blocked, even if not fully delivered in the MVP;
- logs and diagnostics remain technically canonical in English while UI explanations may be localized;
- official documentation may be translated later, but English remains the canonical version for architecture and contracts.

## 2. Context

Chronicle has several distinct language concerns:

1. technical identifiers;
2. user interface language;
3. narrative language;
4. Rule Set terminology;
5. user-authored content;
6. documentation language;
7. diagnostic language;
8. package-localized resources.

Combining these concerns would create instability.

Examples of problematic coupling include:

- persisting translated state names;
- using localized event keys;
- changing database values when the UI locale changes;
- forcing narrative output to follow the desktop UI language;
- treating translated Rule Set labels as field identities;
- storing localized error messages instead of stable error codes.

## 3. Decision Drivers

The architecture prioritizes:

1. stable contracts;
2. open-source contributor accessibility;
3. localization readiness;
4. user freedom of narrative language;
5. deterministic persistence;
6. package neutrality;
7. testability;
8. future internationalization;
9. no post-MVP localization rewrite;
10. clear canonical documentation.

## 4. Language Domains

Chronicle separates:

```text
Technical Language
UI Locale
Narrative Language
Rule Set Display Locale
User Content Language
Documentation Locale
```

## 5. Technical Language

Technical language is always English.

## 6. Technical Identifiers

The following use English:

```text
Campaign
Session
Act
Scene
NarrativeTurn
ProviderAttempt
DiceRoll
OperationRecord
WorkItem
AwaitingPlayerRelease
RecoveryRequired
roll.requested
provider.authentication-failed
```

## 7. No Translation of Technical Keys

Technical keys are never translated.

## 8. UI Locale

UI locale controls:

- menus;
- buttons;
- labels;
- dialogs;
- status explanations;
- accessibility labels;
- help text;
- validation messages;
- recovery guidance.

## 9. Base Locale

The base locale is:

```text
en
```

or a selected canonical English locale such as:

```text
en-US
```

The exact regional base must be chosen consistently.

## 10. Fallback

Missing localized resources fall back to English.

## 11. No Raw English in Views

User-facing strings should not be hardcoded directly in views or view models, except narrowly approved development placeholders.

## 12. Resource Keys

Resources use stable semantic keys.

Examples:

```text
navigation.campaigns
dice.awaiting-player-release
backup.recovery-material-required
security.database-encrypted
provider.retrying
```

## 13. Resource Key Stability

Changing display wording does not require changing the key.

## 14. No Sentence as Key

Avoid using full English sentences as resource identifiers.

## 15. Resource Organization

Recommended resource groups:

```text
Common
Navigation
Campaigns
Characters
Narrative
Dice
Providers
BackupRestore
Security
Packages
Validation
Errors
Accessibility
Installer
```

## 16. Presentation Ownership

UI localization resources belong to:

```text
Chronicle.Presentation.Desktop
```

or the relevant future Presentation project.

## 17. Application Results

Application returns:

- stable error code;
- safe structured parameters;
- preservation state;
- recovery actions.

Presentation converts them into localized text.

## 18. No Localized Application Error

Application must not return user-facing translated prose as its authoritative error contract.

## 19. Error Example

Canonical result:

```text
Code:
    provider.credential-missing

Parameters:
    ProviderProfileName

Preservation:
    CampaignStateUnchanged
```

Presentation localizes it.

## 20. Logging Language

Logs use English technical keys and safe structured fields.

## 21. Log Message Stability

Prefer structured event IDs over free-form prose.

## 22. Diagnostic Localization

The UI may localize diagnostics summaries.

Exported developer diagnostics retain canonical English keys.

## 23. Narrative Language

Narrative language is independent from UI locale.

## 24. Narrative Preference

A Campaign or user preference defines the desired narrative language.

## 25. Example

The desktop UI may be English while the Campaign narrative is Portuguese.

## 26. Provider Request

Narrative Intelligence requests explicitly declare:

```text
RequestedNarrativeLanguage
```

or an equivalent provider-neutral field.

## 27. Narrative Contract Keys

The narrative output contract remains English even when prose is Portuguese, Japanese, or another language.

## 28. Example Narrative Output

```json
{
  "completionStatus": "AwaitingPlayerRelease",
  "narrativeBlocks": [
    {
      "kind": "Narration",
      "text": "A porta cede com um estalo seco."
    }
  ],
  "structuredEvents": [
    {
      "type": "roll.requested"
    }
  ]
}
```

## 29. No Translated Event Type

The event remains:

```text
roll.requested
```

not a localized equivalent.

## 30. Content Preservation

Persisted narrative and user-authored text remains in the language in which it was created.

## 31. No Automatic Historical Translation

Changing narrative language does not translate:

- prior Messages;
- Campaign Memories;
- Character histories;
- Knowledge records;
- user notes;
- archived scenes.

## 32. Future Translation Feature

A future explicit translation operation may create derived translated content.

It must preserve the original.

## 33. Character Names

Names are preserved exactly as authored.

## 34. Unicode

Chronicle uses Unicode throughout.

## 35. Normalization

Identifiers generated by Chronicle use a documented normalization policy.

User content should not be destructively normalized beyond what storage and security require.

## 36. Search

Search must handle Unicode safely.

## 37. Case Rules

Technical identifiers use ordinal, locale-independent comparison where appropriate.

## 38. User Text Comparison

User-facing search may use locale-aware comparison.

## 39. Rule Set Terminology

Rule Set field identities use stable technical keys.

## 40. Example

Canonical field key:

```text
character.attribute.strength
```

Localized display:

```text
English:
    Strength

Portuguese:
    Força
```

## 41. Package Localization

Rule Set packages may provide:

- field labels;
- operation labels;
- help text;
- Dice terminology;
- validation explanations;
- package documentation snippets.

## 42. Package Resource Contract

Package resources are keyed by canonical package-owned semantic identifiers.

## 43. No Localized Package Identity

PackageId and operation keys remain unchanged across locales.

## 44. Missing Package Translation

Fallback order:

1. selected package locale;
2. language fallback;
3. package base English resources;
4. canonical technical key as last-resort diagnostic display.

## 45. Package Terminology Overrides

A Campaign may select package-approved terminology variants without changing field identity.

## 46. Narrative Terminology

Narrative prompts may include localized Rule Set terminology.

## 47. Technical Versus Narrative Term

The provider may narrate “Força” while the structured payload uses:

```text
character.attribute.strength
```

## 48. Localization Format

Use the localization mechanism supported by the selected desktop UI framework.

## 49. Abstraction Requirement

Presentation code should access resources through a stable localization service abstraction.

## 50. Recommended Interface

```text
ILocalizer
```

or the selected framework's equivalent behind an Application-independent Presentation boundary.

## 51. Formatting

Localized text uses locale-aware formatting for:

- dates;
- times;
- numbers;
- file sizes;
- percentages;
- list conjunctions.

## 52. Persisted Time

Timestamps remain UTC and locale-independent in storage.

## 53. Persisted Numbers

Numeric persistence uses invariant formats or native database types.

## 54. Dice Notation

Canonical Dice notation is contract-defined.

Display may localize surrounding text but must not change mechanical meaning.

## 55. Pluralization

Resource architecture must support plural forms.

## 56. Gender and Grammar

Do not build messages through naive concatenation that prevents correct grammar.

## 57. Sentence Construction

Prefer full resource templates with named parameters.

## 58. Parameter Safety

Parameters are never interpreted as localization keys unless explicitly typed as such.

## 59. Rich Text

Localized rich text must use a constrained safe markup format.

## 60. No Executable Localization

Localization resources cannot contain executable code, database queries, provider instructions, or arbitrary script.

## 61. Accessibility

Localized UI must preserve:

- accessible names;
- keyboard hints;
- focus order;
- screen-reader meaning;
- noncolor status cues.

## 62. Layout Resilience

UI layouts must tolerate:

- longer translations;
- shorter translations;
- font fallback;
- line wrapping;
- high DPI;
- larger text settings.

## 63. No Fixed-Width Text Assumption

Important labels and buttons must not depend on English string length.

## 64. Right-to-Left Readiness

MVP does not have to ship an RTL locale.

The architecture must not hardcode left-to-right assumptions into contracts or persistence.

## 65. Pseudolocalization

Development builds should support pseudolocalization.

## 66. Pseudolocalization Purpose

It detects:

- hardcoded strings;
- clipped layouts;
- concatenated grammar;
- missing resources;
- insufficient expansion space.

## 67. Locale Selection

Locale selection follows:

1. explicit Chronicle preference;
2. operating-system locale where supported;
3. English fallback.

## 68. Locale Persistence

The selected UI locale is installation-local preference.

## 69. Campaign Narrative Language Persistence

Narrative language belongs to Campaign or appropriate user preference scope.

## 70. No UI Locale in Campaign Identity

Changing desktop locale does not mutate Campaign identity or history.

## 71. Installer Localization

Installer source text uses English as base.

## 72. Installer Locale

Additional installer translations may be added later.

## 73. Recovery Language

Recovery and Safe Mode must always have an English fallback, even if a localized resource pack fails.

## 74. Security Language

Security-critical confirmations use reviewed resource templates.

## 75. Irreversible Action Warnings

Warnings for:

- complete data deletion;
- recovery-key loss;
- restore replacement;
- package trust;
- external provider disclosure;

must not be assembled from ambiguous fragments.

## 76. Documentation

Architecture, RFCs, ADRs, contracts, and contributor guides are canonical in English.

## 77. Documentation Translations

Translations may exist.

They must identify:

- source version;
- translation status;
- canonical English document;
- last synchronization date.

## 78. Conflict Rule

If a translated technical document conflicts with English:

```text
Canonical English document wins
```

## 79. User Documentation

User-facing guides may be authored or translated for supported locales.

## 80. Code Comments

Code comments use English.

## 81. Commit and Pull Request Language

Repository governance should prefer English for commits, issues, and pull requests to support global collaboration.

## 82. Internal Team Conversation

This ADR does not restrict the natural language used by contributors in informal discussion.

## 83. Database Schema

Tables, columns, indexes, migrations, and constraints use English.

## 84. Persisted Enum Values

Persisted state keys remain English and canonical.

## 85. Migration Stability

Adding a locale never requires schema migration merely to translate identifiers.

## 86. Export Contracts

Portable export uses English technical keys.

## 87. Exported Narrative

Narrative text remains in its original language.

## 88. Import

Import must not translate technical keys according to the current UI locale.

## 89. Provider Errors

Provider adapters return canonical failure keys.

## 90. Provider Native Messages

Provider-native messages may be preserved transiently for diagnostics but are not the canonical user-facing error.

## 91. User Input Language

Chronicle accepts user input in the Campaign's chosen language or any language the provider and UI can process.

## 92. Language Detection

Automatic language detection may assist UX but must not silently change the Campaign narrative-language preference.

## 93. Mixed-Language Campaigns

Chronicle must not reject mixed-language content.

## 94. Translation and Copyright

Translation does not remove copyright or provenance requirements from Rule Set content.

## 95. Unauthorized Content

Localized Rule Set resources must not contain unauthorized sourcebook text.

## 96. Testing Strategy

The implementation requires:

```text
Resource Completeness Tests
Fallback Tests
Hardcoded String Tests
Pseudolocalization Tests
Layout Tests
Pluralization Tests
Formatting Tests
Narrative Language Tests
Package Localization Tests
Persistence Invariance Tests
Export and Import Tests
Accessibility Tests
Architecture Tests
```

## 97. Resource Completeness Tests

Verify base English resources exist for every required key.

## 98. Fallback Tests

Missing translations fall back to English without breaking the workflow.

## 99. Hardcoded String Tests

Production views are scanned for unapproved hardcoded user-facing text.

## 100. Pseudolocalization Tests

Critical workflows render under expanded and accented pseudo-locales.

## 101. Layout Tests

Test:

- long labels;
- multiline errors;
- high DPI;
- large text;
- narrow windows;
- recovery dialogs;
- Dice cards.

## 102. Narrative Language Tests

Verify:

- Portuguese narrative with English UI;
- English narrative with Portuguese UI;
- mixed-language user content;
- technical event keys unchanged.

## 103. Package Localization Tests

A synthetic Rule Set package provides two locales without changing field IDs or operation keys.

## 104. Persistence Invariance Tests

Changing locale must not alter:

- state values;
- event keys;
- package IDs;
- operation fingerprints;
- database schema;
- Dice evidence;
- NarrativeTurn contracts.

## 105. Export and Import Tests

A Campaign exports and imports across installations with different UI locales without semantic changes.

## 106. Accessibility Tests

Localized accessibility labels must remain complete and meaningful.

## 107. Architecture Tests

Architecture tests must reject:

- localized strings as state values;
- localized event types;
- UI resource dependency in Domain;
- Presentation localization types in Application contracts;
- hardcoded Rule Set labels used as identities;
- locale-sensitive Operation fingerprints;
- database schema names generated from translations;
- automatic historical translation during locale change.

## 108. Error Model

Recommended localization errors:

```text
localization.locale-unsupported
localization.resource-missing
localization.resource-format-invalid
localization.parameter-missing
localization.package-resource-missing
localization.fallback-used
localization.recovery-fallback-required
```

## 109. Data Preservation State

Localization operations should state:

```text
AuthoritativeDataUnchanged
LocalePreferenceUpdated
NarrativeLanguagePreferenceUpdated
OriginalContentPreserved
EnglishFallbackUsed
ResourceRejected
```

## 110. Logging

Safe localization logs may include:

- locale key;
- resource key;
- package ID;
- fallback path;
- formatting error code.

They must not log private user text merely because formatting failed.

## 111. Prohibited Patterns

### 111.1 Localized State Values

Persist canonical English keys.

### 111.2 UI Language Equals Narrative Language

Keep preferences separate.

### 111.3 Translate Historical Content Automatically

Preserve originals.

### 111.4 Full Sentence as Resource Key

Use semantic keys.

### 111.5 String Concatenation for Grammar

Use complete templates.

### 111.6 Rule Set Label as Field Identity

Use stable field keys.

### 111.7 Domain Depends on Localization Framework

Localization belongs to Presentation and package resources.

### 111.8 Locale-Sensitive Hashing

Use invariant canonical forms.

### 111.9 Missing Translation Blocks Recovery

Always retain English fallback.

### 111.10 Translation Removes Provenance Requirement

Localized package content still needs provenance.

## 112. Alternatives Considered

### Portuguese as the Technical Base Language

Rejected because English provides broader ecosystem and open-source compatibility, while Portuguese remains fully supported as narrative and future UI language.

### UI Locale Drives Narrative Language

Rejected because users may want different interface and storytelling languages.

### Persist Localized Enum Text

Rejected because locale changes would mutate technical truth and break interoperability.

### Delay Localization Architecture

Rejected because hardcoded UI strings and identity coupling would create later rework.

### Translate All Existing Campaign Content on Locale Change

Rejected because translation is lossy, expensive, and changes authored history.

## 113. Consequences

### Positive

- stable technical contracts;
- no localization-driven schema changes;
- UI can expand to new languages;
- narrative language remains user-controlled;
- Rule Set terminology can be localized safely;
- open-source contribution remains accessible;
- recovery always has a base-language fallback;
- no post-MVP localization foundation rewrite.

### Negative

- resource management begins in MVP;
- UI testing surface increases;
- translators require context and review;
- separate UI and narrative language settings add product decisions;
- package authors must maintain resource keys and fallbacks.

## 114. Risks

### English Leaks into Localized UI

Mitigation:

- hardcoded string scans;
- pseudolocalization;
- fallback reporting.

### Translation Changes Mechanical Meaning

Mitigation:

- canonical technical keys;
- package review;
- terminology glossary;
- contract tests.

### Layout Breaks Under Longer Text

Mitigation:

- resilient layouts;
- pseudo-locales;
- visual tests.

### Narrative Provider Ignores Requested Language

Mitigation:

- explicit request contract;
- output validation where feasible;
- user-visible retry or correction.

## 115. Technology Spike

Before acceptance, implement:

1. base English resource system;
2. localization service abstraction;
3. locale preference;
4. separate narrative-language preference;
5. English fallback;
6. pseudolocalization;
7. one secondary test locale;
8. localized error mapping;
9. package resource contract;
10. locale-invariant persistence tests;
11. mixed UI/narrative language test;
12. recovery fallback test;
13. hardcoded-string scanner;
14. architecture tests.

## 116. Spike Acceptance

The spike passes when:

- the desktop UI runs from English resources;
- one secondary locale can replace UI text;
- missing resources fall back safely;
- Portuguese narrative works with English UI;
- technical keys remain unchanged across locales;
- a Rule Set field displays localized labels with one stable identity;
- export and import work across different UI locales;
- recovery works with only the English resource set;
- no Domain or Application contract depends on Presentation localization types;
- intentionally localized state values fail architecture tests.

## 117. Definition of Compliance

An implementation complies when:

- English is the canonical technical language;
- UI localization uses semantic resource keys;
- English is the fallback locale;
- UI locale and narrative language are separate;
- persisted content preserves its authored language;
- technical identifiers are never translated;
- Rule Set display terminology is localized without changing identity;
- Application returns stable codes rather than translated authoritative errors;
- recovery always works with English resources;
- adding a locale requires no Core schema or contract change;
- localization foundations exist in MVP.

## 118. Review Triggers

Review this ADR if:

- an RTL locale becomes officially supported;
- machine translation becomes a product feature;
- collaborative Campaigns use per-user locales;
- voice narration is introduced;
- package terminology overrides expand;
- a non-English canonical documentation policy is proposed;
- localized provider prompts become package-controlled;
- server-hosted clients introduce locale negotiation.

## 119. Deferred Decisions

Later decisions may define:

- first officially supported non-English UI locale;
- translator workflow;
- translation memory;
- community language packs;
- RTL certification;
- voice locale;
- per-user locale in multiplayer;
- explicit historical translation artifacts;
- package terminology glossary governance.

## 120. Final Decision

Chronicle's technical language is English.

Its interface is localization-ready.

Its stories may be told in any language.

The language of a Campaign will not rewrite the language of its contracts.

The language of the UI will not rewrite the Campaign's history.

The MVP will begin with the localization boundary the complete product needs.
