# Architecture Decision Records

ADRs document concrete architecture and technology decisions.

## Reading Order

1. Start with [ADR-0001: Technology Stack and Framework Selection](ADR-0001-Technology-Stack-and-Framework-Selection.md).
2. Read [ADR-0002: Repository and Solution Structure](ADR-0002-Repository-and-Solution-Structure.md) for architecture semantics.
3. Read [ADR-0043: Repository Topology, Project Boundaries, and Solution Structure](ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md) for current concrete topology.
4. Read technology ADRs as needed for persistence, UI, CI, security, packaging, and governance.

## Authority Rules

- ADR-0002 remains authoritative for repository and solution architecture semantics.
- ADR-0043 v0.2.0 is authoritative for current concrete topology, paths, project names, project boundaries, and dependency layout.
- If older concrete topology examples conflict with ADR-0043 v0.2.0, ADR-0043 v0.2.0 prevails.

## Status and Supersession

Every ADR should include status metadata.

Before using an ADR as implementation authority, check:

- `status`;
- `version`;
- `supersedes`;
- `superseded_by`;
- `depends_on`.

Do not change architectural semantics without a recorded decision.
