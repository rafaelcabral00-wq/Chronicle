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
