# Design Court

Design Court is a multi-agent technical court for software design documents.

It reviews RFCs, ADRs, architecture notes, OpenAPI specifications, and technical user stories through a structured review procedure. Every final finding must be evidence-bound, judge-ruled, actionable, and measurable.

## Status

Design Court is in the v0.1 rigor skeleton phase.

Implemented now:

- .NET 10 solution structure.
- Core domain models for findings, evidence, verification steps, and Judge verdicts.
- Candidate finding and judged finding separation.
- Finding invariant validation.
- Markdown section parsing with stable section IDs.
- Quote and fuzzy quote verification.
- Deterministic Operations Engineer Agent.
- Evidence-gated Judge Agent.
- Local review workflow.
- Markdown report writer.
- Minimal CLI entrypoint.
- Unit and functional tests for core invariants, evidence verification, and seeded RFC review.

Not implemented yet:

- LLM provider integration.
- Evaluation command.
- GitHub Action.

## Quick Start

```powershell
dotnet build DesignCourt.slnx
dotnet test DesignCourt.slnx
dotnet run --project src/DesignCourt.Cli -- review samples/rfcs/payment-rfc-missing-rollback.md
```

The CLI parses the document, runs the deterministic Operations Engineer Agent, gates findings through the Judge, and writes artifacts to `design-court-output/`.

See [docs/testing.md](docs/testing.md) for the functional test and code review checklist.

## Architecture

The core domain is independent from agent frameworks, model providers, GitHub APIs, and file-system implementations.

```text
DesignCourt.Core
  <- DesignCourt.Parsing
  <- DesignCourt.Reporting
  <- DesignCourt.Cli
```

Future infrastructure adapters, including Microsoft Agent Framework integration, must depend on project-owned abstractions instead of leaking framework types into `DesignCourt.Core`.

## Project Layout

```text
src/
  DesignCourt.Cli/
  DesignCourt.Core/
  DesignCourt.Agents/
  DesignCourt.Parsing/
  DesignCourt.Reporting/
tests/
  DesignCourt.Agents.Tests/
  DesignCourt.Core.Tests/
  DesignCourt.Parsing.Tests/
agent-roles/
docs/
samples/
```

## License

Apache-2.0
