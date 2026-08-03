---
id: ADR-0044
title: Open-Source License and Contribution Provenance
status: Proposed
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Governance
supersedes: []
superseded_by: null
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
  - RFC-0003
  - RFC-0004
  - RFC-0030
  - RFC-0035
  - RFC-0042
  - RFC-0043
  - ADR-0001
  - ADR-0002
  - ADR-0012
  - ADR-0028
  - ADR-0037
  - ADR-0038
  - ADR-0041
  - ADR-0042
  - ADR-0043
implements:
  - Open-source repository licensing
  - Contribution provenance
  - Documentation licensing
  - Official package licensing boundaries
  - Third-party notice requirements
related_to:
  - Official Werewolf MVP package specification
  - Trademark policy
  - Dependency review policy
---

> **"Open source must explain not only what others may do with Chronicle, but also what every contributor had the right to give."**

# Open-Source License and Contribution Provenance

## 1. Status

**Proposed**

This ADR selects the licensing and contribution-provenance model for Chronicle's public repository.

The decision is:

- license Chronicle-owned software source code under the **Apache License, Version 2.0**;
- use the SPDX identifier:
  ```text
  Apache-2.0
  ```
- license Chronicle-owned documentation prose and diagrams under the **Creative Commons Attribution 4.0 International License**;
- use the SPDX identifier:
  ```text
  CC-BY-4.0
  ```
- license source-code examples and machine-executable snippets contained in documentation under Apache-2.0 unless a file explicitly says otherwise;
- accept contributions under the same outbound license that applies to the contributed area;
- require contributor provenance through the **Developer Certificate of Origin, Version 1.1**;
- require a `Signed-off-by` attestation for commits entering protected branches;
- not require copyright assignment;
- not introduce a Contributor License Agreement for the initial project;
- allow contributors to retain copyright in their contributions;
- require contributors to have the legal right to submit every contribution;
- treat AI-assisted contributions as the contributor's responsibility under the same provenance rules;
- keep project names, logos, marks, and release identity outside the software and documentation license grants except where legally necessary to use unmodified official distributions;
- require a separate trademark policy before public Stable release;
- keep third-party content under its original license;
- require explicit provenance and notice metadata for every third-party asset, dependency, excerpt, package resource, and generated artifact distributed by Chronicle;
- prohibit unauthorized copyrighted sourcebook text, artwork, logos, fonts, audio, or other protected game materials from entering the repository or official packages;
- license only Chronicle-owned code in the official Werewolf package under Apache-2.0;
- require separate legal and provenance approval for every system-specific term, text, asset, and reference distributed with that package;
- permit the application framework and official packages to have different content inventories while preserving one explicit license manifest per artifact;
- include the required license and notice files in source and binary distributions;
- require license compatibility review for new dependencies;
- block official distribution of code or assets whose rights, obligations, or provenance cannot be established;
- describe the decision as a project governance policy rather than legal advice.

The decision becomes **Accepted** after:

- the project owner approves the license choices;
- the root repository includes the required license files;
- the contribution workflow enforces DCO sign-off;
- package manifests support license and provenance metadata;
- dependency review checks exist;
- release artifacts contain the required notices;
- a synthetic unlicensed asset fails validation;
- the official Werewolf package passes a separate legal-content inventory review;
- the project has a documented trademark and official-distribution identity policy.

## 2. Context

Chronicle is intended to be an open-source RPG framework and desktop application.

The public repository is expected to contain:

- application source;
- Domain and Application code;
- desktop Presentation code;
- persistence and Infrastructure code;
- package SDKs;
- official Rule Set implementation code;
- automated tests;
- build and release scripts;
- architecture documents;
- RFCs and ADRs;
- user and contributor documentation;
- diagrams;
- synthetic samples;
- generated contract artifacts;
- binary release definitions.

The project also interacts with content that may have different legal ownership:

- third-party dependencies;
- provider SDKs;
- tabletop RPG terminology;
- game-system names;
- trademarks;
- sourcebook text;
- artwork;
- fonts;
- sound;
- translations;
- user-created Campaign content;
- Narrative Intelligence output;
- contributor-created code and documentation.

A repository may be publicly visible without granting a valid open-source license.

Chronicle must therefore state:

- what may be used;
- what may be modified;
- what may be redistributed;
- what attribution must remain;
- what patent rights are granted;
- what contributors certify;
- what is not licensed;
- what remains owned by third parties;
- which notices must accompany releases;
- how official and unofficial distributions are distinguished.

## 3. Decision Drivers

The licensing decision prioritizes:

1. genuine open-source status;
2. broad adoption;
3. commercial and noncommercial use;
4. explicit patent terms;
5. low contribution friction;
6. contributor copyright retention;
7. traceable contribution provenance;
8. compatibility with a package ecosystem;
9. clear documentation reuse;
10. protection against unauthorized sourcebook distribution;
11. official-distribution clarity;
12. practical open-source maintenance.

## 4. License Model Summary

Chronicle will use:

```text
Software code
    Apache-2.0

Tests, scripts, schemas, code samples
    Apache-2.0

Documentation prose and diagrams
    CC-BY-4.0

Documentation code blocks
    Apache-2.0 unless explicitly marked otherwise

Official Rule Set implementation code
    Apache-2.0 where Chronicle owns the code

Third-party content
    original license only

Project names, logos, and marks
    separate trademark policy

Contributions
    inbound license matches outbound license

Contributor provenance
    DCO 1.1 sign-off

Copyright assignment
    none

CLA
    none initially
```

## 5. Software License Selection

Chronicle-owned software is licensed under Apache License, Version 2.0.

## 6. SPDX Identifier

The canonical software-license identifier is:

```text
Apache-2.0
```

## 7. Software Scope

Apache-2.0 applies by default to Chronicle-owned:

- C# source files;
- project files;
- build scripts;
- CI scripts;
- database migrations;
- package manifests;
- schemas;
- contract definitions;
- test code;
- test-support code;
- synthetic fixtures;
- source-code generators;
- command-line tools;
- package SDK source;
- official package implementation code;
- source-code examples;
- installer scripts;
- update scripts;
- developer tooling.

## 8. Apache-2.0 Rationale

Apache-2.0 is selected because it provides a permissive open-source model while including explicit patent-license terms and redistribution obligations.

## 9. Permissive Distribution

The selected license allows Chronicle code to be:

- used;
- studied;
- modified;
- redistributed;
- included in commercial products;
- included in noncommercial products;
- distributed in source form;
- distributed in object form;

subject to the license conditions.

## 10. Patent Grant

Apache-2.0 includes an express patent grant from contributors for patent claims necessarily infringed by their contributions, subject to the license terms.

## 11. Patent Litigation Termination

The selected license includes patent-license termination behavior for specified patent litigation.

## 12. Why Not MIT as the Primary License

MIT is simple and permissive.

Apache-2.0 is preferred because its express patent terms and NOTICE mechanism better match a framework intended to accept contributions, adapters, packages, and commercial use.

## 13. Why Not GPL for the Core Framework

A strong copyleft license would provide different downstream obligations.

Chronicle currently prioritizes:

- embedding;
- package development;
- commercial and noncommercial adoption;
- provider and desktop integration;
- broad reuse of the framework.

Apache-2.0 better matches that initial goal.

## 14. Why Not AGPL for the Core Framework

Chronicle is currently local-first, but future hosting is possible.

The project is not selecting network copyleft as an adoption condition.

A future governance review may reconsider this only through a new ADR and only for future copyrightable contributions.

## 15. Irrevocability

Open-source license grants are not temporary product permissions.

Contributors and maintainers must understand that validly licensed released versions remain available under their published licenses.

## 16. No Retroactive Relicensing Assumption

Chronicle does not assume it can later relicense all existing contributions unilaterally.

## 17. Future Dual Licensing

Dual licensing is not part of the initial model.

Introducing it would require:

- rights analysis;
- contributor agreement analysis;
- public governance decision;
- a new ADR;
- no reduction of rights already granted for released versions.

## 18. Documentation License Selection

Chronicle-owned documentation prose and diagrams are licensed under Creative Commons Attribution 4.0 International.

## 19. Documentation SPDX Identifier

The canonical documentation-license identifier is:

```text
CC-BY-4.0
```

## 20. Documentation Scope

CC-BY-4.0 applies by default to Chronicle-owned:

- architecture prose;
- RFC prose;
- ADR prose;
- contributor guides;
- user guides;
- explanatory diagrams;
- tutorials;
- non-executable examples;
- website documentation;
- release documentation;
- package-development guides.

## 21. Documentation Reuse

The documentation license permits sharing and adaptation subject to its attribution requirements and other terms.

## 22. Documentation Attribution

Redistributors of Chronicle documentation must preserve appropriate attribution, license notice, and change indication according to CC-BY-4.0.

## 23. Documentation Code Blocks

Code blocks, command examples, JSON, schemas, and executable snippets in documentation are licensed under Apache-2.0 unless explicitly marked otherwise.

## 24. Mixed Documentation Files

A documentation file may contain:

```text
Prose
    CC-BY-4.0

Executable code examples
    Apache-2.0
```

The repository must explain this mixed-content rule in a central licensing file.

## 25. Documentation Generated from Source

Generated API documentation inherits the applicable license of:

- source comments;
- templates;
- generated prose;
- included code.

## 26. Diagrams Containing Code

A diagram that primarily reproduces source code or executable schema may be treated as Apache-2.0.

A conceptual architecture diagram remains CC-BY-4.0.

## 27. Root License Files

The repository SHOULD include:

```text
LICENSE
LICENSE-DOCUMENTATION
NOTICE
THIRD-PARTY-NOTICES
TRADEMARKS
```

## 28. `LICENSE`

`LICENSE` contains the complete Apache License 2.0 text.

## 29. `LICENSE-DOCUMENTATION`

`LICENSE-DOCUMENTATION` contains or clearly identifies the complete CC-BY-4.0 legal code.

## 30. `NOTICE`

`NOTICE` contains attribution notices required for Chronicle and inherited Apache-licensed components when applicable.

## 31. `THIRD-PARTY-NOTICES`

`THIRD-PARTY-NOTICES` inventories distributed third-party components and required notices.

## 32. `TRADEMARKS`

`TRADEMARKS` describes:

- official project marks;
- permitted nominative use;
- unofficial distribution naming;
- no-confusion rules;
- logo and release-badge use.

The exact policy requires review before Stable release.

## 33. File-Level License Headers

Chronicle-owned source files SHOULD use a concise SPDX header where practical.

Recommended:

```text
SPDX-License-Identifier: Apache-2.0
```

## 34. Documentation Headers

Chronicle-owned standalone documentation MAY use:

```text
SPDX-License-Identifier: CC-BY-4.0
```

when this does not interfere with document frontmatter or rendering.

## 35. Frontmatter License Field

Architecture and user documents MAY instead declare:

```yaml
license: CC-BY-4.0
code_examples_license: Apache-2.0
```

## 36. Header Exceptions

Generated files, third-party files, and formats that do not support comments may use:

- directory-level metadata;
- manifest metadata;
- adjacent license files;
- generated-file headers.

## 37. No Incorrect Project Header on Third-Party Files

Chronicle must not place its own Apache-2.0 header on content it does not own or have authority to relicense.

## 38. Copyright Ownership

Contributors retain copyright in their contributions unless a separate lawful agreement says otherwise.

## 39. No Copyright Assignment

Chronicle does not require contributors to transfer copyright to the project owner.

## 40. Copyright Notice

Repository notices may use a collective form such as:

```text
Copyright Chronicle contributors
```

without claiming exclusive ownership of every contribution.

## 41. Contributor License Model

The project uses an inbound-equals-outbound model.

## 42. Inbound Software Contributions

A contribution to Apache-2.0 code is submitted under Apache-2.0.

## 43. Inbound Documentation Contributions

A contribution to CC-BY-4.0 documentation is submitted under CC-BY-4.0.

## 44. Mixed Contribution

A pull request containing code and documentation contributes each part under the license applicable to its repository area.

## 45. No Implicit Relicensing of Third-Party Content

A contributor cannot submit third-party content under Chronicle's licenses unless the contributor has the right to do so.

## 46. Developer Certificate of Origin

Chronicle adopts Developer Certificate of Origin, Version 1.1.

## 47. DCO Purpose

DCO sign-off records the contributor's certification that the contributor has the right to submit the contribution under the applicable open-source license.

## 48. Signed-Off-By Trailer

Commits entering protected branches require:

```text
Signed-off-by: Contributor Name <email@example.invalid>
```

## 49. Sign-Off Meaning

The sign-off is a legal attestation.

It is not merely a formatting preference.

## 50. Sign-Off Scope

The sign-off applies to:

- code;
- documentation;
- tests;
- assets;
- generated contract updates;
- package content;
- translations;
- build scripts.

## 51. DCO Automation

Continuous integration SHOULD validate sign-off automatically.

## 52. Pull Requests from Forks

DCO checks apply equally to:

- maintainers;
- outside contributors;
- automated dependency updates where legally appropriate;
- pull requests from forks.

## 53. Squash Merge Policy

If squash merging is used, the repository must preserve valid contribution sign-off in the resulting commit or use a hosting workflow that keeps DCO evidence.

## 54. Bot Contributions

Automated bots may contribute only when:

- their operator has configured valid provenance;
- generated changes have a known source;
- applicable license terms are preserved;
- a human reviewer accepts the change.

## 55. No Initial CLA

Chronicle will not require a Contributor License Agreement initially.

## 56. CLA Rationale

A CLA adds legal and contributor friction.

The initial Apache-2.0 plus DCO model is sufficient for the intended community workflow.

## 57. Future CLA

A CLA may be introduced only through a new ADR explaining:

- the concrete legal need;
- affected contribution types;
- treatment of earlier contributions;
- governance;
- contributor impact.

## 58. No Retroactive CLA Assumption

Earlier contributions cannot be assumed to be governed by a later CLA without valid agreement.

## 59. Contributor Representation

By signing off, a contributor certifies rights under the DCO terms.

## 60. Contributor Responsibility

Contributors are responsible for verifying that their contribution does not contain:

- employer-owned code without permission;
- copied proprietary code;
- unauthorized sourcebook text;
- incompatible third-party content;
- leaked credentials;
- private user data;
- confidential material;
- unlawfully obtained assets.

## 61. Employer Contributions

Contributors working within employment obligations are responsible for obtaining required authorization.

## 62. Commissioned Contributions

A contributor submitting commissioned work must have the right to license it to Chronicle.

## 63. Translations

Translators must have the right to contribute both:

- the translation;
- any source material being translated.

## 64. AI-Assisted Contributions

AI assistance does not remove contributor responsibility.

## 65. AI Output Review

A contributor using generative tools must review the resulting contribution for:

- correctness;
- provenance risk;
- copied or suspiciously similar code;
- unauthorized protected text;
- insecure behavior;
- license incompatibility;
- personal or confidential data.

## 66. AI Disclosure

A pull request SHOULD disclose material AI assistance when it meaningfully generated code, prose, tests, or assets.

## 67. AI Contribution Authority

The human or legally accountable contributor remains the submitting party under DCO.

## 68. No Provider Output Dump

Raw model output from private Campaigns or provider conversations must not be committed as a contribution.

## 69. Generated Assets

AI-generated images, audio, or other assets are not accepted into official distribution merely because they were generated.

They require:

- provenance review;
- provider-term review;
- similarity review where practical;
- explicit approval;
- license metadata.

## 70. User-Created Campaign Content

The Chronicle software licenses do not claim ownership of user-created Campaign content.

## 71. Campaign Content License

Users retain whatever rights they hold in:

- Campaign prose;
- Character descriptions;
- Memories;
- custom Rule Knowledge;
- imported materials;
- generated narrative.

## 72. No Automatic Open-Source Publication

Saving content in Chronicle does not license that content to the public repository.

## 73. Diagnostic Submission

A user sharing a diagnostic or reproduction artifact grants only the permissions required by the explicit support process.

The repository license does not automatically apply to private submitted Campaign data.

## 74. Synthetic Test Data

Committed test fixtures must be synthetic or validly licensed.

## 75. Sample Campaign License

Chronicle-created sample Campaigns may be licensed under CC-BY-4.0, with executable schemas and scripts under Apache-2.0.

## 76. Third-Party Dependencies

Every distributed dependency retains its original license.

## 77. Dependency Inventory

The build must generate or validate an inventory containing:

```text
Dependency
Version
Source
License identifier
License file
Notice requirement
Distribution scope
Review status
```

## 78. Dependency Approval

A new dependency requires review for:

- license compatibility;
- redistribution obligations;
- patent terms;
- attribution;
- source availability requirements;
- binary distribution impact;
- maintenance;
- security;
- necessity.

## 79. Permissive Dependencies

Common permissive licenses may be accepted after normal review.

Examples include:

```text
Apache-2.0
MIT
BSD-2-Clause
BSD-3-Clause
ISC
CC0-1.0
```

Actual acceptance remains dependency-specific.

## 80. Copyleft Dependencies

GPL, AGPL, LGPL, MPL, and other reciprocal licenses require explicit compatibility and distribution review.

## 81. Strong Copyleft in Official Binary

Strong-copyleft dependencies must not be added to the official Chronicle binary without a dedicated legal and architecture decision.

## 82. Weak and File-Level Copyleft

Weak or file-level copyleft dependencies require review of:

- linking;
- modification;
- source distribution;
- notice;
- replacement rights;
- package boundaries.

## 83. Proprietary Dependencies

A proprietary runtime dependency is incompatible with the default open-source distribution goal unless a dedicated ADR approves it.

## 84. Provider SDKs

Provider SDKs require the same dependency and redistribution review as all other libraries.

## 85. Build Tools

Build-only tools also require license review when redistributed or embedded in artifacts.

## 86. Fonts

Fonts are separate licensed works.

Every distributed font must include:

- license;
- source;
- attribution;
- embedding rights;
- redistribution permission.

## 87. Images and Icons

Every distributed image and icon must be:

- Chronicle-created;
- generated and approved under a valid license;
- public domain;
- or third-party licensed for the intended distribution.

## 88. Audio

Audio assets require explicit provenance and redistribution rights.

## 89. Third-Party License Files

Required third-party license texts and notices accompany source or binary distribution as required.

## 90. NOTICE Preservation

When redistributing Apache-licensed works with a NOTICE file, Chronicle will preserve applicable attribution notices in a readable form.

## 91. NOTICE Scope

NOTICE must not be used to impose new licensing restrictions.

## 92. Third-Party Notice Generation

The release pipeline SHOULD generate a candidate third-party notice inventory and require human review for Stable releases.

## 93. Software Bill of Materials

Official releases SHOULD include or publish a software bill of materials.

## 94. SBOM Is Not the License Notice

An SBOM supports inventory.

It does not replace required license texts or notices.

## 95. Official Rule Set Packages

Official Rule Set packages are separate distributable artifacts with explicit license metadata.

## 96. Package Code

Chronicle-owned implementation code in an official Rule Set package is licensed under Apache-2.0.

## 97. Package Content

System-specific names, prose, images, data, examples, and terminology may have different legal ownership.

## 98. No Blanket Relicensing

Apache-2.0 on package code does not relicense third-party game-system content.

## 99. Werewolf Package Boundary

The official Werewolf package may contain only content whose use and redistribution are approved by the package's provenance inventory.

## 100. Exact Game Edition

The exact game edition and legal basis must be selected in the official Werewolf MVP package specification.

## 101. Sourcebook Text

The package must not reproduce sourcebook text unless Chronicle has explicit authorization or a clearly reviewed lawful basis.

## 102. Original Mechanical Implementation

Chronicle may implement original code expressing selected game mechanics only after legal and provenance review.

## 103. Mechanics Versus Expression

The project must distinguish:

- abstract game mechanics;
- original Chronicle implementation;
- copyrighted explanatory expression;
- trademarks and branding;
- protected artwork and layout.

## 104. Trademarked Game Names

Use of third-party game names and marks does not imply ownership, sponsorship, or endorsement.

## 105. Official Package Naming

Package naming must follow the legal-content and trademark decision in the Werewolf package specification.

## 106. Distribution Block

If the legal right to distribute a system-specific package is uncertain, the package must not be included in official binaries or releases.

## 107. Framework Independence

The generic Chronicle framework remains independently buildable without proprietary or legally uncertain Rule Set content.

## 108. Package Manifest License Fields

Every package manifest SHOULD include:

```text
PackageLicense
LicenseFile
DocumentationLicense
ContentProvenanceManifest
ThirdPartyNotices
TrademarkNotices
SourceReference
```

## 109. Content Provenance Manifest

The package provenance manifest SHOULD identify:

```text
Content item
Owner or source
License or authorization
Distribution scope
Modification status
Attribution
Review record
```

## 110. User-Installed Packages

Chronicle may allow users to install packages that are not licensed under Apache-2.0.

## 111. Package Runtime Neutrality

Chronicle's package loader does not claim ownership of user-installed packages.

## 112. Package License Display

The UI SHOULD display package license and provenance metadata before installation or activation.

## 113. Unsupported Package Terms

Chronicle may reject packages with missing or malformed license metadata according to package policy.

## 114. Marketplace

A future marketplace requires additional content, trademark, takedown, and licensing governance.

## 115. Trademarks

The software and documentation licenses do not grant unrestricted rights to use Chronicle's names, logos, icons, release badges, or other source-identifying marks.

## 116. Trademark Policy Requirement

A separate trademark policy must exist before Stable public release.

## 117. Nominative Use

The trademark policy should permit truthful reference to:

- Chronicle compatibility;
- Chronicle forks;
- Chronicle documentation;
- Chronicle-derived packages;

without implying official status.

## 118. Modified Distributions

A materially modified distribution must not represent itself as an official Chronicle release.

## 119. Fork Naming

Forks may state that they are based on Chronicle.

They should use distinct branding when needed to prevent confusion.

## 120. Official Release Badge

Only project-authorized artifacts may use official release badges or signing identity.

## 121. Project Logo

The project logo is not automatically licensed under Apache-2.0 or CC-BY-4.0 for independent branding use.

## 122. Required Functional Use

The trademark policy may allow unmodified official binaries to include their normal names and logos.

## 123. No Endorsement

Redistribution must not imply endorsement by Chronicle maintainers or unrelated game publishers.

## 124. Binary Distribution

Official binary distributions must include:

- Apache-2.0 license;
- documentation-license notice where documentation is included;
- NOTICE;
- third-party notices;
- package license manifests;
- applicable attribution;
- source or source-location information where policy requires it.

## 125. Source Distribution

Source distributions include the complete license and contribution files.

## 126. Installer

The installer must expose or install applicable license notices.

## 127. Portable Archive

A portable executable archive must contain the same required license and notice inventory as the installer.

## 128. Package Archive

Every package archive carries its own license and provenance metadata.

## 129. Documentation Site

A documentation site must display its CC-BY-4.0 licensing and code-example exception clearly.

## 130. Release Notes

Release notes identify material license or third-party notice changes.

## 131. Modified Redistribution

A redistributor must preserve notices required by Apache-2.0 and third-party licenses.

## 132. Change Notices

Modified files should carry appropriate notices when required by the applicable license.

## 133. Source Availability

Apache-2.0 does not require redistributors to publish modified source code.

Chronicle does not add a separate source-publication condition.

## 134. Commercial Use

Commercial use is permitted under Apache-2.0 and CC-BY-4.0 subject to their terms.

## 135. Hosted Use

Hosting Chronicle-derived software does not create an additional source-publication condition under the selected software license.

## 136. Project Sustainability

The permissive license does not prevent Chronicle maintainers from offering:

- support;
- hosted services;
- consulting;
- packaged distributions;
- sponsorship;
- commercial integrations.

## 137. Official Versus Compatible

Commercial or community distributions must distinguish:

```text
Official Chronicle release
Chronicle-compatible distribution
Derived from Chronicle
Unofficial fork
```

## 138. Security Fixes

Security fixes are contributed under the same applicable license.

## 139. Embargoed Contributions

A security contribution may remain private during coordinated disclosure.

When published, its provenance and license must be documented.

## 140. Vulnerability Reports

A vulnerability report is not automatically licensed as source code.

The security process must handle disclosure and publication rights explicitly.

## 141. Generated Files

Generated files inherit the license determined by:

- the source definition;
- the generator license;
- included templates;
- included third-party content.

## 142. Generated File Metadata

Committed generated files should identify:

```text
Generator
Source
Applicable license
Regeneration command
```

## 143. Generator Does Not Override Input Rights

Running a Chronicle generator does not grant rights to unlicensed input content.

## 144. Database Migrations

Chronicle-authored database migrations are Apache-2.0.

## 145. Serialized User Data

The software license does not apply merely because user data uses Chronicle's schema.

## 146. Schemas

Chronicle-authored schemas and executable contract definitions are Apache-2.0 unless explicitly documented otherwise.

## 147. Public API Documentation

Explanatory prose is CC-BY-4.0.

API signatures and source-derived code representations follow Apache-2.0.

## 148. Architecture Documents

RFCs, ADRs, reviews, diagrams, and indexes are CC-BY-4.0, with embedded code examples under Apache-2.0.

## 149. Contribution Guide

`CONTRIBUTING.md` must explain:

- applicable licenses;
- DCO sign-off;
- no copyright assignment;
- prohibited content;
- dependency review;
- AI-assisted contribution responsibility;
- documentation versus code licensing;
- package provenance.

## 150. Pull-Request Template

The pull-request template SHOULD ask:

```text
I have the right to submit this contribution.
My commits include DCO sign-off.
I have identified third-party content.
I have not included secrets or private Campaign data.
I have not copied unauthorized sourcebook text or assets.
I have disclosed material AI assistance.
I have updated license or notice files when required.
```

## 151. Issue Templates

Issue templates must not encourage users to upload copyrighted books, secrets, or private Campaign artifacts.

## 152. Code Review

Reviewers inspect licensing risk in addition to technical correctness.

## 153. License-Sensitive Paths

Changes in these paths require explicit provenance review:

```text
packages/
samples/
docs/
assets/
fonts/
installer/
third-party/
generated fixtures/
```

## 154. CODEOWNERS

The repository MAY use ownership rules to require a licensing or package maintainer review for sensitive paths.

## 155. Dependency Pull Requests

A dependency update must preserve or update:

- license inventory;
- notices;
- SBOM;
- binary distribution review.

## 156. License Scan

Continuous integration SHOULD run license detection and policy validation.

## 157. Scan Limitations

Automated license scanning is evidence, not final legal determination.

## 158. Unknown License

A dependency or asset with unknown licensing is blocked from official distribution.

## 159. Custom License

A custom or nonstandard license requires explicit review.

## 160. License Conflict

A license conflict blocks merge or distribution until resolved.

## 161. Missing Attribution

Missing required attribution blocks Stable release.

## 162. Repository Secret Scan

Secret scanning remains separate but related to provenance enforcement.

## 163. Content Similarity Review

For system-specific prose and generated assets, maintainers may require a similarity review against protected source material.

## 164. Takedown Process

The project SHOULD document a contact and process for rights holders to report allegedly infringing content.

## 165. Takedown Is Not Automatic Admission

A report triggers review and preservation of relevant records.

## 166. Emergency Removal

Maintainers may temporarily remove a disputed artifact from distribution while legal status is assessed.

## 167. Git History

Removing a file in a new commit may not remove it from repository history.

Rights-sensitive incidents may require history rewriting and credential or artifact invalidation.

## 168. Release Withdrawal

A release containing improperly distributed content may be withdrawn.

## 169. Artifact Revocation

Withdrawn artifacts should be marked as withdrawn while preserving safe verification metadata.

## 170. Legal Review Boundary

Maintainers may seek qualified legal review for:

- third-party game content;
- trademarks;
- custom licenses;
- copyleft compatibility;
- commercial distribution;
- takedown;
- contributor disputes.

## 171. Not Legal Advice

This ADR is an engineering and governance decision.

It is not a substitute for jurisdiction-specific legal advice.

## 172. License Compatibility Policy

The repository SHOULD maintain a machine-readable or reviewed policy classifying licenses as:

```text
Allowed
AllowedWithNotice
ReviewRequired
NotAllowedForOfficialDistribution
Unknown
```

## 173. No Universal License Shortcut

A license family cannot always be approved without examining:

- version;
- exception;
- linking model;
- modification;
- distribution form;
- included notices.

## 174. Review Record

A dependency or content review SHOULD record:

```text
Item
Version
License
Reviewer
Date
Decision
Conditions
Evidence
```

## 175. Stable Release Gate

Stable release is blocked when:

- required license files are missing;
- DCO violations remain unresolved;
- dependency license state is unknown;
- third-party notices are incomplete;
- package provenance is incomplete;
- unauthorized content is detected;
- official branding policy is absent;
- artifact license inventory differs from the approved inventory.

## 176. Preview Release Gate

Preview releases follow the same copyright and license requirements as Stable.

Preview status does not permit unlicensed distribution.

## 177. Development Build

A local Development build may reference developer-owned test content that is not distributed.

Such content must remain outside the repository and official artifacts.

## 178. CI Artifacts

Pull-request artifacts are still distributions.

They must not contain unauthorized content, credentials, or assets.

## 179. Fork Pull Requests

Fork-based CI must not receive signing keys or privileged distribution credentials.

## 180. Contribution Rejection

Maintainers may reject a technically valid contribution because its provenance or licensing is unclear.

## 181. Contributor Correction

A contributor may replace disputed content with an original or properly licensed alternative.

## 182. DCO Failure

A contribution without required sign-off cannot merge into a protected branch.

## 183. Sign-Off Correction

A contributor must add or amend sign-off through a traceable workflow.

Maintainers should not forge contributor sign-off.

## 184. Co-Authored Contributions

Every co-author whose contribution is included should provide valid provenance evidence according to repository policy.

## 185. Pair Programming

Pair-programmed contributions should record authorship and sign-off appropriately.

## 186. Corporate Contributions

Corporate contributors may use internal processes, but Chronicle still requires DCO evidence for submitted commits.

## 187. Maintainer Contributions

Maintainers are subject to the same DCO and provenance rules.

## 188. Historical Imports

Imported code history requires:

- license evidence;
- provenance;
- contributor rights;
- notice preservation;
- compatibility review.

## 189. Code Copying Between Projects

Copying open-source code requires preserving:

- license;
- copyright;
- notices;
- modification marking;
- compatibility obligations.

## 190. Inspiration Versus Copying

Reimplementing an idea does not eliminate the need to avoid copying protected expression.

## 191. Error Model

Recommended policy and validation errors:

```text
licensing.license-file-missing
licensing.spdx-identifier-invalid
licensing.dco-signoff-missing
licensing.provenance-missing
licensing.third-party-license-unknown
licensing.third-party-notice-missing
licensing.dependency-review-required
licensing.license-incompatible
licensing.sourcebook-content-prohibited
licensing.asset-rights-unverified
licensing.trademark-review-required
licensing.generated-content-unverified
licensing.release-inventory-mismatch
licensing.distribution-blocked
```

## 192. Data Preservation State

Licensing validation results SHOULD state:

```text
RepositoryUnchanged
ContributionBlocked
ArtifactNotPublished
ExistingReleaseUnaffected
ReleaseWithdrawn
ProvenanceRecorded
NoticeUpdated
ReviewRequired
```

## 193. Logging

License tooling MAY log:

- file path relative to repository;
- dependency name and version;
- SPDX identifier;
- validation rule;
- review status;
- safe error code.

It MUST NOT log private sourcebook contents, contributor secrets, or confidential legal communications.

## 194. Metrics

Useful governance metrics include:

```text
DcoFailureCount
UnknownLicenseCount
DependencyReviewCount
ThirdPartyNoticeFailureCount
ProvenanceFailureCount
DisputedContentCount
ReleaseLicenseGateFailureCount
```

These metrics must not be used as hidden contributor ranking.

## 195. Testing Strategy

The implementation requires:

```text
Repository License Tests
DCO Tests
Dependency License Tests
Notice Tests
Package Provenance Tests
Artifact Inventory Tests
Documentation License Tests
Generated File Tests
Trademark Metadata Tests
Release Gate Tests
```

## 196. Repository License Tests

Tests MUST verify:

- root Apache-2.0 text exists;
- documentation license exists;
- license policy is discoverable;
- source files use correct metadata where required;
- third-party files are not mislabeled.

## 197. DCO Tests

Tests MUST verify:

- signed-off commits pass;
- missing sign-off fails;
- bot contributions follow policy;
- squash or merge workflow preserves evidence.

## 198. Dependency License Tests

Tests MUST verify:

- known allowed dependency;
- dependency requiring notice;
- unknown license;
- incompatible policy classification;
- changed license between versions.

## 199. Notice Tests

Tests MUST verify:

- NOTICE included in source artifact;
- NOTICE included in binary artifact;
- required dependency attribution preserved;
- stale notice inventory fails.

## 200. Package Provenance Tests

Tests MUST verify:

- package code license;
- content provenance manifest;
- third-party notice links;
- missing source rights;
- prohibited sourcebook excerpt;
- unknown asset owner;
- unsupported trademark metadata.

## 201. Documentation Tests

Tests MUST verify:

- CC-BY-4.0 declaration;
- Apache-2.0 code-example rule;
- generated documentation license metadata;
- third-party image attribution.

## 202. Generated File Tests

Tests MUST verify:

- generator identity;
- source;
- applicable license;
- deterministic regeneration;
- no unlicensed embedded input.

## 203. Artifact Inventory Tests

Tests MUST scan:

- installer;
- portable archive;
- package archives;
- source archive;
- documentation bundle;
- SBOM;
- third-party notices.

## 204. Synthetic Violation Tests

The pipeline SHOULD include synthetic fixtures for:

- missing license;
- unknown dependency license;
- missing DCO;
- unlicensed image;
- copied sourcebook-like text;
- missing NOTICE;
- trademark-confusing package name.

## 205. Required Test Cases

Tests MUST cover:

- Chronicle code contribution;
- documentation contribution;
- mixed code and docs PR;
- dependency addition;
- package asset addition;
- generated fixture;
- AI-assisted declared contribution;
- missing provenance;
- official release artifact;
- unofficial Development artifact;
- withdrawn content scenario;
- no credential or Campaign-data inclusion.

## 206. Architecture Tests

Architecture and repository tests MUST reject:

- official package without license metadata;
- third-party code relabeled as Chronicle-owned;
- sourcebook text committed without approval;
- binary artifact without license files;
- code contribution without DCO;
- Stable release with unknown dependency license;
- provider SDK license omitted from notices;
- project logo treated as freely rebrandable code without policy;
- CLA requirement introduced without an ADR;
- copyright assignment inserted into contribution workflow silently.

## 207. Prohibited Patterns

### 207.1 Public Repository Without a License

Visibility alone is not permission.

### 207.2 One License Header Applied to Everything

Code, documentation, marks, and third-party content have different rules.

### 207.3 Contributor Copyright Assignment by Default

Contributors retain copyright.

### 207.4 Merge Without Provenance

DCO sign-off is required.

### 207.5 Copy Sourcebook Text into Rule Knowledge

Only authorized or independently created content may be distributed.

### 207.6 Assume AI Output Is Automatically Safe to License

The contributor remains responsible.

### 207.7 Depend on Unknown-License Packages

Unknown is not approved.

### 207.8 Hide Third-Party Notices

Required notices must accompany distributions.

### 207.9 Treat Apache-2.0 as a Trademark License

Official marks require separate policy.

### 207.10 Change License Retroactively by Editing `LICENSE`

Released rights and contributor ownership must be respected.

## 208. Alternatives Considered

### MIT for All Source Code

MIT would be simpler, but Apache-2.0 provides more explicit patent terms and a stronger notice framework.

### GPL-3.0 for All Source Code

GPL would preserve stronger source-sharing obligations for distributions, but it would narrow some integration and commercial adoption options.

### AGPL-3.0 for All Source Code

AGPL would add network-use obligations, but hosted Chronicle is not the current product model and broad framework reuse is preferred.

### MPL-2.0

MPL provides file-level copyleft and could be a balanced alternative.

The project currently prefers the simpler permissive ecosystem boundary of Apache-2.0.

### Apache-2.0 for Documentation

Software licenses can cover documentation, but CC-BY-4.0 provides clearer attribution and adaptation language for prose and diagrams.

### CLA and Copyright Assignment

Rejected initially because DCO with inbound-equals-outbound licensing provides lower-friction provenance without centralizing all copyright ownership.

### No DCO

Rejected because the repository needs explicit contributor certification of submission rights.

### One Blanket License for Official Rule Set Content

Rejected because package code and third-party system content may have different owners and permissions.

## 209. Consequences

### Positive

- Chronicle becomes genuinely open source under a recognized permissive license;
- commercial and noncommercial use are allowed;
- contributors retain copyright;
- patent terms are explicit;
- contribution provenance is auditable;
- no CLA is required initially;
- documentation can be reused with attribution;
- official packages carry explicit content provenance;
- unauthorized game content is clearly prohibited;
- dependencies and releases receive consistent license checks;
- official branding remains distinguishable from unofficial forks.

### Negative

- downstream forks are not required to publish source modifications;
- hosted derivatives are not required to disclose source;
- two primary licenses must be explained;
- DCO enforcement adds contribution workflow steps;
- package provenance review requires ongoing maintenance;
- trademark policy is still required;
- legal review may block or delay the official Werewolf package;
- license scanners cannot replace human review.

## 210. Risks

### Contributor Submits Code They Do Not Own

Mitigation:

- DCO;
- contribution checklist;
- code review;
- provenance challenge process;
- removal procedure.

### Official Package Contains Unauthorized Source Material

Mitigation:

- separate package specification;
- provenance manifest;
- no blanket relicensing;
- sourcebook-content scan;
- legal review;
- distribution block.

### Fork Creates Branding Confusion

Mitigation:

- trademark policy;
- official signing identity;
- distinct release badges;
- no-endorsement rule.

### Dependency Changes License

Mitigation:

- version lock;
- license scan;
- dependency update review;
- release inventory diff.

### AI-Assisted Contribution Copies Protected Material

Mitigation:

- contributor responsibility;
- disclosure;
- review;
- similarity analysis where practical;
- removal process.

### DCO Becomes Contributor Friction

Mitigation:

- clear instructions;
- automated feedback;
- hosting-platform sign-off support;
- no additional CLA.

## 211. Technology and Governance Spike

Before acceptance, implement:

1. root Apache-2.0 license file;
2. CC-BY-4.0 documentation license file;
3. central licensing policy;
4. DCO policy and automated check;
5. contribution checklist;
6. SPDX validation;
7. dependency license inventory;
8. NOTICE generation and validation;
9. third-party notice inventory;
10. package provenance manifest schema;
11. artifact license scan;
12. synthetic unauthorized-content fixture;
13. SBOM generation;
14. trademark-policy draft;
15. official Werewolf legal-content inventory template.

## 212. Spike Acceptance

The spike passes when:

- a clean source archive contains the correct licenses;
- a binary artifact contains required notices;
- a documentation bundle identifies CC-BY-4.0;
- code examples are covered by Apache-2.0;
- a commit without DCO sign-off fails;
- a valid signed-off contribution passes;
- an unknown-license dependency blocks release;
- a required NOTICE is preserved;
- a package without provenance metadata fails validation;
- a synthetic unauthorized sourcebook excerpt fails review;
- an unofficial build cannot present itself as an official signed release;
- no CLA or copyright assignment is required.

## 213. Definition of Compliance

An implementation and repository comply when:

- Chronicle-owned software is Apache-2.0;
- Chronicle-owned documentation is CC-BY-4.0;
- executable documentation examples are Apache-2.0 unless marked otherwise;
- all contributors retain copyright and submit under the applicable outbound license;
- every protected-branch contribution has DCO 1.1 provenance;
- no CLA or copyright assignment is silently required;
- third-party dependencies and assets preserve their original licenses and notices;
- unknown or incompatible content is blocked from official distribution;
- the official Werewolf package licenses only Chronicle-owned code and separately inventories every system-specific content item;
- unauthorized sourcebook text and assets are prohibited;
- project trademarks and official release identity remain governed separately;
- source, package, installer, portable, and documentation artifacts include required license material;
- release gates validate license inventory, notices, provenance, and artifact contents.

## 214. Review Triggers

This ADR must be reviewed if:

- the project adopts a CLA;
- copyright assignment is proposed;
- dual licensing is proposed;
- the core software license is proposed to change;
- hosted Chronicle becomes the primary distribution model;
- a package marketplace is introduced;
- a third-party game publisher grants an official content license;
- project ownership or governance changes;
- commercial official packages are introduced;
- contributor patent policy changes;
- a trademark is registered or transferred;
- documentation licensing changes.

## 215. Deferred Decisions

Later decisions MAY define:

- final trademark policy text;
- exact official project marks;
- package marketplace content terms;
- takedown procedure details;
- dual-licensing policy;
- official publisher partnership terms;
- CLA adoption if a concrete need appears;
- long-term support and relicensing governance;
- contributor dispute arbitration;
- jurisdiction-specific legal review procedures.

## 216. Normative External Documents

This ADR relies on the unmodified official texts of:

```text
Apache License, Version 2.0
Creative Commons Attribution 4.0 International
Developer Certificate of Origin, Version 1.1
```

The repository must include or reference the official legal texts rather than paraphrased substitutes.

## 217. Final Decision

Chronicle-owned software will be released under Apache-2.0.

Chronicle-owned documentation will be released under CC-BY-4.0, with executable examples under Apache-2.0.

Contributors will retain their copyright and certify submission rights through DCO 1.1 sign-off.

No CLA or copyright assignment will be required initially.

Third-party content will remain under its own license.

The official Werewolf package will distribute only Chronicle-owned implementation code and separately approved content with explicit provenance.

Open source will not be treated as a label applied after development.

It will be enforced as a traceable chain of permission from contributor, to repository, to package, to release, to user.
