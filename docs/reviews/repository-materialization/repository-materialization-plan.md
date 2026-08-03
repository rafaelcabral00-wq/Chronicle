# Chronicle Repository Materialization Plan

Date: 2026-08-03

## Authority

Normative authority:

- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`

Supporting authority was inspected across ADRs/RFCs for technology stack, project boundaries, dependency direction, testing, packaging, CI, persistence, UI, Rule Set runtime, security, and open-source governance.

## Planned Root Topology

```text
Chronicle/
|-- Chronicle.sln
|-- Directory.Build.props
|-- Directory.Build.targets
|-- Directory.Packages.props
|-- global.json
|-- .editorconfig
|-- .gitignore
|-- LICENSE
|-- NOTICE
|-- CONTRIBUTING.md
|-- SECURITY.md
|-- TRADEMARKS.md
|-- README.md
|-- src/
|-- tests/
|-- rule-sets/
|-- tools/
|-- docs/
|-- samples/
|-- build/
`-- artifacts/
```

## Planned Projects

Production projects:

- `src/Chronicle.Domain/`
- `src/Chronicle.Contracts/`
- `src/Chronicle.RuleSets.Abstractions/`
- `src/Chronicle.NarrativeIntelligence.Abstractions/`
- `src/Chronicle.Application/`
- `src/Chronicle.Infrastructure/`
- `src/Chronicle.Persistence.Sqlite/`
- `src/Chronicle.NarrativeIntelligence.OpenAI/`
- `src/Chronicle.Presentation.Desktop/`
- `src/Chronicle.Desktop/`

Rule Set package source:

- `rule-sets/Chronicle.RuleSets.Werewolf/`

Tests:

- `tests/Chronicle.Domain.Tests/`
- `tests/Chronicle.Application.Tests/`
- `tests/Chronicle.Architecture.Tests/`
- `tests/Chronicle.Contracts.Tests/`
- `tests/Chronicle.Infrastructure.Tests/`
- `tests/Chronicle.Persistence.Sqlite.Tests/`
- `tests/Chronicle.NarrativeIntelligence.ContractTests/`
- `tests/Chronicle.NarrativeIntelligence.OpenAI.Tests/`
- `tests/Chronicle.Presentation.Desktop.Tests/`
- `tests/Chronicle.Desktop.IntegrationTests/`
- `tests/Chronicle.EndToEnd.Tests/`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/`

Tools:

- `tools/Chronicle.Tools.ContractValidator/`
- `tools/Chronicle.Tools.PackageValidator/`
- `tools/Chronicle.Tools.MigrationInspector/`
- `tools/Chronicle.Tools.BackupInspector/`
- `tools/Chronicle.Tools.ProvenanceScanner/`

## Materialization Roles

Documentation prototype:

- planned authority: review and authoring evidence only;
- current path: `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype`;
- not executable, package source, packaged artifact, installed artifact, publication authority, activation authority, or Campaign-binding authority.

Rule Set package source:

- planned path: `rule-sets/Chronicle.RuleSets.Werewolf/`;
- requires explicit transformation, identity preservation, fingerprints, validation, reconciliation, and accepted evidence from the documentation prototype.

Packaged artifact:

- deferred output of a package build;
- not published, installed, enabled, or active by existence.

Installed artifact:

- deferred installed layout under `rule-sets/<package-id>/<package-version>/`;
- created only by installation transition with integrity and compatibility evidence.

Chronicle platform source, tests, and tooling remain separate roots with no hidden authority transfer.

## README Plan

The root `README.md` is a first-class open-source product artifact.

Required sections:

- Project identity
- Current status
- What Chronicle is
- What exists today
- What is not supported yet
- Architecture map
- Rule Set package model
- Werewolf reference status
- Getting started for contributors
- Documentation entry points
- Governance, license, security

Supportable claims:

- Chronicle is an open-source framework in documentation and bootstrap planning.
- The architecture defines a local-first desktop MVP.
- Werewolf is a finalized documentation prototype/reference baseline, not a published package.
- Materialization planning is ready; physical source bootstrap has not started.

BOOT-001 prerequisite selections:

- ADR-0043 is `Accepted` as a clerical/status correction supported by DR-0001.
- .NET SDK line is `.NET 10 LTS`; `global.json` should pin an exact installed `10.0.x` SDK patch or approved `latestFeature` roll-forward policy at execution time.
- Initial GitHub Actions runner is `windows-latest` for the Windows desktop MVP bootstrap. Multi-platform CI remains deferred.
- `CODE_OF_CONDUCT.md` should use Contributor Covenant Code of Conduct version 2.1 with explicit attribution.
- `SECURITY.md` should state that there are no supported production releases yet, route vulnerability reports away from public issues, and use GitHub private vulnerability reporting or owner-controlled placeholder contact without personal data.

Prohibited claims:

- production ready;
- published Werewolf package available;
- dynamic community package marketplace available;
- complete Werewolf source RPG implementation;
- installer available;
- Campaign runtime available.

## Deferred Items

- Dynamic Rule Set loading, marketplace, signing, sandboxing, and remote registry.
- Package artifact creation and publication.
- Installed package layout creation.
- Campaign binding creation.
- Dedicated `Chronicle.RuleSet.Runtime`.
- Generic `Chronicle.RuleKnowledge` production project.
- Release installer workflow.
- Code of conduct text until selected by maintainers.
- Exact package serialization format and final Werewolf PackageId.

## Unresolved Materialization Issues

- `MAT-ISSUE-001`: resolved. ADR-0043 is now Accepted as a clerical/status correction supported by DR-0001.
- `RSM-ISSUE-001`: exact Rule Set package serialization format remains open.
- `RSM-ISSUE-002`: final Werewolf PackageId remains a governance decision.
- `GH-ISSUE-001`: non-blocking. Exact security contact remains owner-controlled until the GitHub repository exists.
- `GH-ISSUE-002`: resolved for BOOT-001. Contributor Covenant 2.1 selected with attribution.
- `CI-ISSUE-001`: resolved for BOOT-001. Use .NET 10 LTS and `windows-latest`; exact SDK patch remains owner-controlled at execution time.

## BOOT-001 Execution Evidence

Status: complete on 2026-08-03.

Created:

- `README.md`
- `LICENSE`
- `CONTRIBUTING.md`
- `CODE_OF_CONDUCT.md`
- `SECURITY.md`
- `SUPPORT.md`
- `CHANGELOG.md`
- `.gitignore`
- `.editorconfig`
- `global.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `Chronicle.sln`
- `docs/README.md`
- `docs/adrs/README.md`
- `docs/rfcs/README.md`
- `docs/specs/README.md`
- `docs/rule-sets/README.md`

Validation:

- `dotnet --version`: passed with `10.0.302`.
- `dotnet sln Chronicle.sln list`: passed; no projects are in the solution.
- BOOT-001 Markdown links: passed.
- Protected Werewolf sourcebook file references: passed.
- Local absolute path, secret, and personal data scan: passed; only policy wording matched.

Not created:

- `src/`
- `tests/`
- `rule-sets/`
- `tools/`
- `.github/`
- `NOTICE`
- `Directory.Build.targets`
- `Chronicle.slnx`

## BOOT-002 Execution Evidence

Status: complete on 2026-08-03.

Updated governance placeholders:

- `SECURITY.md`
- `SUPPORT.md`
- `CODE_OF_CONDUCT.md`

Created directories:

- `src/`
- `tests/`
- `rule-sets/`
- `tools/`
- `build/`
- `samples/`
- `artifacts/`

Created preservation files:

- `src/.gitkeep`
- `tests/.gitkeep`
- `rule-sets/.gitkeep`
- `tools/.gitkeep`
- `build/.gitkeep`
- `samples/.gitkeep`
- `artifacts/.gitkeep`

Validation:

- repository URL and support/security statements: passed;
- authorized paths only: passed;
- protected sourcebook, secret, binary, cache, and local path scan: passed;
- git status: passed; only expected BOOT-002 files and directories changed.

Not created:

- source projects;
- test projects;
- tool projects;
- Rule Set source packages;
- packaged artifacts;
- installed artifacts;
- GitHub workflows.

## Bootstrap Readiness

Physical repository bootstrap is executable as a scoped bootstrap task, provided it creates only planned root files, canonical directories, solution/project skeletons, and CI scaffolding.

It must not publish or install packages, create Campaign bindings, select unapproved package serialization or installation layouts, or treat the Werewolf documentation prototype as package source.

Current next executable bootstrap task: `BOOT-005`, create Werewolf package source skeleton and package-adjacent tests after explicit authorization.

## BOOT-003 Execution Evidence

Status: complete on 2026-08-03.

Created production project skeletons:

- `src/Chronicle.Domain/`
- `src/Chronicle.Application/`
- `src/Chronicle.Contracts/`
- `src/Chronicle.RuleSets.Abstractions/`
- `src/Chronicle.NarrativeIntelligence.Abstractions/`
- `src/Chronicle.Infrastructure/`
- `src/Chronicle.Persistence.Sqlite/`
- `src/Chronicle.NarrativeIntelligence.OpenAI/`
- `src/Chronicle.Presentation.Desktop/`
- `src/Chronicle.Desktop/`

Created bootstrap test project skeletons:

- `tests/Chronicle.Domain.Tests/`
- `tests/Chronicle.Application.Tests/`
- `tests/Chronicle.Architecture.Tests/`
- `tests/Chronicle.Contracts.Tests/`
- `tests/Chronicle.Infrastructure.Tests/`
- `tests/Chronicle.Persistence.Sqlite.Tests/`

Validation:

- `dotnet --version`: passed with `10.0.302`.
- `dotnet sln Chronicle.sln list`: passed with 16 projects.
- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed.
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed.
- `dotnet test Chronicle.sln --no-build`: passed for current placeholder test assemblies.
- Project reference graph: passed; acyclic and no forbidden bootstrap references found.
- Generated output staging: passed; `bin/` and `obj/` outputs are ignored and not staged.

Not created:

- Rule Set package source.
- Werewolf executable package.
- Werewolf package tests.
- Tool projects.
- Packaged artifacts.
- Installed artifacts.
- Campaign bindings.
- Release outputs.

Next executable bootstrap task: `BOOT-004`, implement canonical test harness packages and architecture rules without expanding runtime behavior.

## BOOT-004 Execution Evidence

Status: complete on 2026-08-03.

Packages added through central package management:

- `Microsoft.NET.Test.Sdk` 17.12.0
- `xunit` 2.9.3
- `xunit.runner.visualstudio` 2.8.2

Test infrastructure added:

- xUnit harness configuration for the six bootstrap test projects.
- Deterministic test-discovery convention tests for non-architecture bootstrap test projects.
- Project-file graph inspection architecture tests in `tests/Chronicle.Architecture.Tests/`.

Architecture rules implemented:

- Domain depends on no Chronicle production project.
- Contracts depends on no concrete infrastructure project.
- Application does not depend on Infrastructure, Persistence, provider implementations, Presentation, or Desktop.
- Abstractions do not depend on concrete implementations.
- Infrastructure and Persistence do not depend on Presentation or Desktop.
- Presentation does not own persistence or provider implementations.
- Desktop is the composition root and may depend only on authorized projects.
- No circular project references.
- Test projects do not introduce forbidden production dependencies.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed.
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors.
- `dotnet test Chronicle.sln --no-build`: passed with 15 tests.
- Intentional forbidden dependency check: passed; temporary `Chronicle.Application -> Chronicle.Infrastructure` reference failed architecture tests, then the valid state was restored.
- Generated output staging: passed; `bin/`, `obj/`, and local NuGet scratch outputs are ignored and not staged.

Not created:

- Rule Set package source.
- Werewolf executable package.
- Werewolf package tests.
- Tool projects.
- UI, database, provider, or Rule Set runtime logic.

Next executable bootstrap task: `BOOT-005`, create Werewolf package source skeleton and package-adjacent tests after explicit authorization.

## BOOT-005 CI Execution Evidence

Status: complete on 2026-08-03.

Created:

- `.github/workflows/ci.yml`

Workflow:

- runner: `windows-latest`
- .NET SDK: `10.0.302`
- steps: checkout, setup .NET, restore `Chronicle.sln`, build `Chronicle.sln`, test `Chronicle.sln`

Validation:

- workflow YAML syntax: passed;
- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 15 tests;
- README unsupported-claims check: passed; README was not modified;
- secrets, personal data, provider credential, and OpenAI-call check: passed for the workflow.

Not created:

- pull request template;
- issue templates;
- cache;
- matrix;
- package, installer, release, publish, signing, deployment, or artifact upload steps.

Next executable implementation task: create the Werewolf package source skeleton and package-adjacent tests after explicit authorization, or first reconcile the BOOT numbering if CI is retained as BOOT-005 in the task graph.

## IMPLEMENT-001 Execution Evidence

Status: complete on 2026-08-03.

Created package source:

- `rule-sets/Chronicle.RuleSets.Werewolf/Chronicle.RuleSets.Werewolf.csproj`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfRuleSetPackage.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`

Created package-adjacent tests:

- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/Chronicle.RuleSets.Werewolf.Tests.csproj`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfPackageSourceTests.cs`

Package-source mapping:

- documentation prototype: `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype`;
- package source: `rule-sets/Chronicle.RuleSets.Werewolf`;
- package-adjacent tests: `rule-sets/Chronicle.RuleSets.Werewolf.Tests`;
- copied review/evidence ledgers: no;
- copied raw source/sourcebook text: no.

PackageId status:

- value used: `chronicle.rulesets.werewolf`;
- status: provisional governance-pending.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 21 tests;
- package dependency graph: passed; Werewolf source references only `Chronicle.RuleSets.Abstractions` and `Chronicle.Contracts`;
- Core dependency boundary: passed; architecture tests verify Core projects do not reference `Chronicle.RuleSets.Werewolf`;
- no prototype review/evidence ledgers copied into package source;
- no raw sourcebook text or protected source file included;
- no network, persistence, secret, UI, or provider dependency in package source.

Deferred:

- dynamic loading;
- publication, promotion, installation, packaged artifact creation, and Campaign binding;
- persistence, UI, provider access, and network access;
- additional Gift purchase implementation;
- runtime Gift effects;
- full source-system completeness.

Next executable implementation task: implement package manifest contract types or package source validation tooling after explicit authorization.

## IMPLEMENT-002 Execution Evidence

Status: complete on 2026-08-03.

Created manifest contract and validation types:

- `RuleSetPackageManifest`
- `RuleSetScopeDeclaration`
- `RuleSetCompatibilityDeclaration`
- `RuleSetCapabilityDeclaration`
- `RuleSetDisabledOperationDeclaration`
- `RuleSetManifestSchema`
- `RuleSetManifestValidationResult`
- `RuleSetManifestValidationError`
- `RuleSetManifestValidationErrorCode`
- `RuleSetManifestValidator`

Files:

- `src/Chronicle.RuleSets.Abstractions/Manifests/RuleSetManifestContracts.cs`
- `src/Chronicle.RuleSets.Abstractions/Manifests/RuleSetManifestValidation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetManifestValidatorTests.cs`

Werewolf manifest changes:

- added explicit compatibility declaration;
- added disabled-operation capability keys;
- normalized deterministic ordering for capabilities and excluded mechanics;
- retained provisional `chronicle.rulesets.werewolf` PackageId with `provisional-governance-pending` status.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 31 tests;
- architecture rules: passed;
- package boundary scan: passed;
- generated output staging: passed.

Deferred:

- package discovery;
- dynamic loading;
- publication, installation, and Campaign binding;
- package serialization format;
- final Werewolf PackageId;
- schema file publication or external schema tooling.

Next executable task: implement package source validation tooling or package manifest loader integration after explicit authorization.

## IMPLEMENT-003 Execution Evidence

Status: complete on 2026-08-03.

Created package source validation contracts and validator:

- `RuleSetPackageSourceValidationResult`
- `RuleSetPackageSourceFinding`
- `RuleSetPackageSourceFindingSeverity`
- `RuleSetPackageSourceErrorCode`
- `RuleSetPackageSourceValidator`

Files:

- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetPackageSourceValidatorTests.cs`

Werewolf package source changes:

- no package source files changed;
- the existing Werewolf package source is now covered by deterministic package-source validation tests.

Validation coverage:

- required manifest presence and parsing;
- manifest contract validation;
- declared localization resource existence;
- required package-source files;
- path containment under package root;
- duplicate, missing, malformed, and undeclared resources;
- prohibited review/evidence/sourcebook files;
- forbidden binaries, secrets, generated outputs, and absolute paths;
- disabled operation enforcement;
- deterministic file inventory and finding ordering;
- no filesystem mutation.

Validation:

- `dotnet test rule-sets/Chronicle.RuleSets.Werewolf.Tests/Chronicle.RuleSets.Werewolf.Tests.csproj --no-build --filter FullyQualifiedName~RuleSetPackageSourceValidatorTests`: passed with 14 tests;
- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 45 tests;
- architecture rules: passed;
- Werewolf boundaries: passed; no persistence, presentation, provider, network, secret, protected source, or prototype-evidence dependency accepted;
- generated output staging: passed.

Deferred:

- package discovery;
- dynamic assembly loading;
- publication, installation, and Campaign binding;
- packaged artifact serialization;
- final PackageId governance;
- runtime implementation details.

Next executable task: integrate package source validation into authorized tooling or continue with the next package contract work item after explicit authorization.

## IMPLEMENT-004 Execution Evidence

Status: complete on 2026-08-03.

Authorization result:

- `docs/reviews/repository-materialization/tools-project-plan.json` authorizes `Chronicle.Tools.PackageValidator`;
- allowed dependencies: Rule Set abstractions and contracts;
- forbidden behavior: executing untrusted arbitrary package code without security review.

Created projects:

- `tools/Chronicle.Tools.PackageValidator/Chronicle.Tools.PackageValidator.csproj`
- `tests/Chronicle.Tools.PackageValidator.Tests/Chronicle.Tools.PackageValidator.Tests.csproj`

Created files:

- `tools/Chronicle.Tools.PackageValidator/PackageValidatorExitCode.cs`
- `tools/Chronicle.Tools.PackageValidator/PackageValidatorCommand.cs`
- `tools/Chronicle.Tools.PackageValidator/Program.cs`
- `tests/Chronicle.Tools.PackageValidator.Tests/PackageValidatorCommandTests.cs`

Command usage:

- `dotnet run --project tools/Chronicle.Tools.PackageValidator/Chronicle.Tools.PackageValidator.csproj -- <package-source-path>`

Exit codes:

- `0`: valid package source;
- `1`: validation failure;
- `2`: invalid invocation;
- `3`: unexpected internal failure.

Tests added:

- valid Werewolf package;
- invalid package;
- missing path;
- path outside repository when explicitly supplied;
- deterministic output;
- stable exit codes;
- no filesystem mutation;
- no forbidden runtime/provider/persistence/UI dependencies.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed after approved NuGet network access for the new test project;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 53 tests;
- architecture rules: passed;
- real invocation against `rule-sets/Chronicle.RuleSets.Werewolf`: passed with exit code `0`, `Status: valid`, and `Findings: 0`;
- generated output staging: passed.

Deferred:

- canonical JSON output;
- package discovery;
- dynamic assembly loading;
- package installation and publication;
- Campaign binding;
- packaged artifact serialization;
- final PackageId governance.

Next executable task: integrate package validation into CI only if explicitly authorized, or continue the next Rule Set package contract work item.

## IMPLEMENT-005 Execution Evidence

Status: complete on 2026-08-03.

Created discovery contracts and service:

- `RuleSetPackageSourceDiscoveryRequest`
- `RuleSetPackageSourceDiscoveryResult`
- `RuleSetPackageSourceDescriptor`
- `RuleSetPackageSourceRejection`
- `RuleSetPackageSourceDiscoveryValidationStatus`
- `RuleSetPackageSourceDiscoveryErrorCode`
- `RuleSetPackageSourceDiscoveryService`

Files:

- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceDiscovery.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetPackageSourceDiscoveryTests.cs`

Discovery behavior:

- requires one or more explicit authorized search roots;
- inspects only direct child candidate package-source directories;
- identifies candidates by canonical manifest presence;
- invokes the canonical package-source validator;
- returns immutable validated descriptors and rejections;
- records package path, manifest identity, version, declared scope, capabilities, validation status, and findings;
- preserves deterministic ordering;
- detects duplicate PackageId/version candidates.

Tests added:

- valid Werewolf package discovery from `rule-sets/`;
- empty root;
- nonexistent root;
- malformed candidate;
- valid and invalid candidates together;
- duplicate identity/version candidates;
- deterministic ordering;
- no traversal outside authorized roots;
- no filesystem mutation;
- no forbidden runtime/provider/persistence/UI dependencies.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed after approved NuGet network access for repository signature metadata;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 63 tests;
- architecture rules: passed;
- real discovery against `rule-sets/`: passed through focused discovery test;
- generated output staging: passed.

Deferred:

- final PackageId governance;
- dynamic loading;
- packaged artifact serialization;
- Desktop or Campaign integration;
- package installation, publication, or activation.

Next executable task: expose controlled discovery through authorized tooling, or continue the next Rule Set package contract work item after explicit authorization.

## IMPLEMENT-006 Execution Evidence

Status: complete on 2026-08-03.

Created registration and catalog contracts:

- `RuleSetPackageRegistrationRequest`
- `RuleSetPackageRegistrationResult`
- `RegisteredRuleSetPackageDescriptor`
- `RuleSetPackageRegistrationRejection`
- `RuleSetPackageRegistrationStatus`
- `RuleSetPackageRegistrationErrorCode`
- `RuleSetPackageCatalog`
- `RuleSetPackageRegistrationService`

Files:

- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageRegistration.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetPackageRegistrationTests.cs`
- updated `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceDiscovery.cs` to carry registration evidence and manifest fields.

Registration behavior:

- accepts only validated discovered descriptors with canonical validation evidence;
- rejects invalid, rejected, or manually fabricated descriptors lacking evidence;
- registers identity, version, scope, compatibility, localization, capabilities, disabled operations, and source location;
- exposes immutable catalog snapshots;
- supports lookup by PackageId and PackageId/version;
- detects duplicate PackageId/version registrations;
- distinguishes available, compatible, incompatible, and rejected outcomes;
- preserves deterministic ordering.

Tests added:

- registering the discovered Werewolf package;
- rejecting a nonvalidated descriptor;
- duplicate PackageId/version;
- multiple versions of one PackageId;
- compatible and incompatible contract versions;
- immutable catalog results;
- deterministic ordering;
- lookup by identity and version;
- no filesystem mutation or implicit discovery;
- forbidden dependency boundaries;
- focused discovery to registration to catalog lookup flow.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 74 tests;
- architecture rules: passed;
- focused flow against `rule-sets/`: passed.

Deferred:

- final Werewolf PackageId governance;
- dynamic assembly loading;
- runtime Rule Set behavior;
- Desktop or Campaign integration;
- packaged artifact serialization;
- package installation, publication, or activation.

Next executable task: expose registration/catalog inspection through authorized tooling, or continue the next Rule Set package contract work item after explicit authorization.

## IMPLEMENT-007 Execution Evidence

Status: complete on 2026-08-03.

Created runtime boundary contracts and services:

- `RuleSetRuntimeIdentity`
- `RuleSetRuntimeMetadata`
- `RuleSetOperationDescriptor`
- `RuleSetOperationStatus`
- `RuleSetOperationRequest`
- `RuleSetOperationResult`
- `RuleSetRuntimeFinding`
- `RuleSetOperationFailureCode`
- `IRuleSetRuntime`
- `RuleSetRuntimeRegistry`
- `RuleSetRuntimeRegistrationService`

Files:

- `src/Chronicle.RuleSets.Abstractions/Runtime/RuleSetRuntimeContracts.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`
- updated `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` to declare the explicit runtime skeleton as package source.

Werewolf runtime skeleton:

- matches provisional package `chronicle.rulesets.werewolf` version `0.1.0-source-skeleton`;
- enabled operation: `character-creation.create-character`;
- disabled operations: `character-creation.purchase-additional-gift`, `gift-runtime.execute-gift-effect`;
- enabled operation returns deterministic `OperationNotImplemented` until dedicated implementation tasks.

Validation:

- `dotnet restore Chronicle.sln --disable-parallel /p:BuildInParallel=false`: passed;
- `dotnet build Chronicle.sln --no-restore /m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false`: passed with 0 warnings and 0 errors;
- `dotnet test Chronicle.sln --no-build /m:1 /p:BuildInParallel=false`: passed with 85 tests;
- architecture rules: passed;
- focused real Werewolf flow: passed.

Deferred:

- character creation behavior;
- dice behavior;
- Campaign integration;
- persistence and UI;
- dynamic loading;
- packaged artifact serialization;
- final Werewolf PackageId governance.

Next executable task: implement the first enabled Werewolf current-slice operation behavior behind the runtime boundary, or expose runtime/catalog inspection through authorized tooling after explicit authorization.
