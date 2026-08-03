# Chronicle

Chronicle is an open-source, local-first tabletop RPG platform for durable solo and small-group campaigns.

It is built around a simple promise: AI may help narrate the world, but it does not own the rules, the dice, the database, or campaign truth. Chronicle keeps mechanical authority deterministic, provider integrations replaceable, and long-running campaign state under the player's control.

## Vision

Chronicle aims to make persistent tabletop role-playing easier to sustain. A campaign should survive interrupted sessions, provider failures, character growth, rule checks, corrections, and years of accumulated history without becoming a pile of chat logs.

The platform is designed for:

- deterministic mechanics and auditable dice evidence;
- local-first persistence and privacy-respecting defaults;
- replaceable Narrative Intelligence providers;
- durable Campaign history owned by Chronicle, not by a provider transcript;
- community-authored Rule Sets with explicit provenance, validation, and compatibility contracts;
- a desktop-first MVP that can grow without turning the first implementation into the architecture.

## Current Status

Chronicle is under active runtime development.

The architecture documentation and repository materialization planning are complete enough to begin bootstrap work. The Werewolf reference baseline is finalized as documentation and authoring evidence for the first official Rule Set workflow.

There is not yet an installable application, working release, published package, screenshot set, or passing CI pipeline.

## Principles

- Chronicle owns Campaign truth.
- Rule Sets own system-specific mechanics.
- Chronicle generates and records dice evidence; Rule Sets interpret it.
- AI providers assist narration and structured suggestions, but deterministic state changes are validated by Chronicle.
- Persistence is local-first and durable.
- Providers, Rule Sets, persistence, and UI remain replaceable through explicit boundaries.
- Documentation prototypes are evidence, not executable package authority.

## Architecture Overview

Chronicle is planned as a modular .NET desktop application:

- `src/` will contain Chronicle platform source.
- `tests/` will contain platform tests and architecture enforcement.
- `rule-sets/` will contain official Rule Set package source after explicit materialization.
- `docs/` contains ADRs, RFCs, specifications, extraction records, and reconciliation plans.

The current topology authority is [ADR-0043](docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md). Rule Set package authority is defined by [RFC-0027](docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md) and [SPEC-0001](docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md).

## Roadmap

1. Root governance and build bootstrap.
2. Canonical source, test, Rule Set, tooling, build, sample, and artifact directories.
3. Empty platform project skeletons and solution wiring.
4. Architecture tests for dependency direction.
5. Werewolf package source skeleton, transformed from the documentation baseline through the approved materialization contract.
6. CI bootstrap without release or publication workflows.

## Development Prerequisites

Required:

- Git.
- .NET 10 SDK, not only the runtime.
- VS Code or another compatible editor.
- C# tooling for the selected editor.
- Python 3.12 or newer for prototype validation harnesses.

Recommended:

- PowerShell 7.
- VS Code C# Dev Kit.
- VS Code Python extension.
- GitHub integration in VS Code or another editor.

Optional:

- GitHub CLI.

You do not currently need:

- Visual Studio;
- Docker;
- SQL Server;
- a manually installed SQLite server;
- Node.js for the Chronicle runtime;
- cloud infrastructure;
- OpenAI credentials merely to build the repository.

## Windows Setup

Install the required tools, then verify from the repository root:

```powershell
git --version
dotnet --version
python --version
```

For the current bootstrap, `dotnet --version` should resolve to `10.0.302`.

Python launcher installations may expose multiple versions. Check both commands if present:

```powershell
python --version
py --version
```

GitHub CLI is optional. If GitHub is integrated through VS Code, contributors can clone, branch, commit, push, and open pull requests without installing `gh`.

For the complete contributor setup, see [CONTRIBUTING.md](CONTRIBUTING.md).

## Documentation

- [Documentation index](docs/README.md)
- [ADRs](docs/adrs/README.md)
- [RFCs](docs/rfcs/README.md)
- [Specifications](docs/specs/README.md)
- [Rule Set documentation](docs/rule-sets/README.md)
- [Repository materialization plan](docs/reviews/repository-materialization/repository-materialization-plan.md)

## Contributing

Chronicle welcomes architecture review, documentation improvements, validation harness work, test design, package authoring tools, and implementation contributions once the corresponding bootstrap phase is opened.

Start with [CONTRIBUTING.md](CONTRIBUTING.md), then review the [Code of Conduct](CODE_OF_CONDUCT.md), [Security Policy](SECURITY.md), and [Support Policy](SUPPORT.md).

## Legal Notice

Chronicle source code is licensed under the Apache License, Version 2.0. Documentation, Rule Set content, third-party marks, and game-system source material may have separate rights and provenance requirements.

Chronicle does not grant rights to third-party tabletop RPG text, trademarks, images, or proprietary source material.
