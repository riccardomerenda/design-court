# Design Court

[![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4.svg)](global.json)
[![Version](https://img.shields.io/github/v/release/riccardomerenda/design-court?include_prereleases&label=version&sort=semver)](docs/releases.md)
[![CI](https://github.com/riccardomerenda/design-court/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/riccardomerenda/design-court/actions/workflows/ci.yml)
[![Status](https://img.shields.io/badge/status-engineering%20preview-f59e0b.svg)](#status)

**A technical court for software design documents.**

Design Court reviews RFCs, ADRs, architecture notes, OpenAPI specifications, and technical user stories through a structured adversarial review procedure.

It is not a generic AI document reviewer. The core promise is procedural rigor:

- every final finding cites document evidence;
- evidence is machine-verified before judgment;
- a Judge accepts, rejects, or escalates findings;
- recommendations must be actionable;
- verification steps must be human-checkable;
- quality is measured against seeded benchmark samples.

## Why This Exists

Software teams make high-impact engineering decisions in writing, but design review quality is often inconsistent. The right senior reviewer may be busy, the risky paragraph may be missed, and generic AI feedback can be noisy or difficult to verify.

Design Court aims to make pre-implementation review more repeatable:

```text
Design document -> Review agents -> Evidence verifier -> Judge -> Report
```

The goal is not to replace human review. The goal is to surface concrete, evidence-backed design risks before implementation starts.

## Status

Design Court is in an early v0.1 engineering preview.

Implemented:

- .NET 10 solution structure.
- Core domain contracts for findings, evidence, verification, and Judge verdicts.
- Candidate finding and judged finding separation.
- Stable finding fingerprints.
- Markdown section parsing with stable section IDs.
- Exact and fuzzy quote verification.
- Deterministic Operations Engineer Agent.
- Evidence-gated Judge Agent.
- Local review workflow.
- Markdown and JSON report output.
- Seeded sample RFC and clean control.
- Benchmark manifest and `design-court eval` with precision, recall, F1, and false-positive rate.
- Unit and functional tests.
- Testing and code review guide.
- SemVer source version and CI workflow.

Not implemented yet:

- LLM provider integration.
- Microsoft Agent Framework adapter.
- GitHub Action.
- Multi-agent MVP roles beyond Operations.

## Quick Start

Prerequisite: [.NET 10 SDK](https://dotnet.microsoft.com/).

```powershell
dotnet build DesignCourt.slnx
dotnet test DesignCourt.slnx
dotnet run --project src/DesignCourt.Cli -- --version
dotnet run --project src/DesignCourt.Cli -- review samples/rfcs/payment-rfc-missing-rollback.md
dotnet run --project src/DesignCourt.Cli -- eval
```

The CLI writes:

```text
design-court-output/
  report.md
  findings.json
```

Current expected console output:

```text
Parsed 5 section(s).
Produced 1 judged finding(s).
Wrote design-court-output/report.md
Wrote design-court-output/findings.json
```

## Example Finding

The seeded RFC contains a destructive database migration without a rollback plan. Design Court currently produces an accepted Operations finding:

```json
{
  "id": "F-OPS-001",
  "title": "Missing rollback strategy for database migration",
  "category": "operability",
  "severity": "high",
  "status": "accepted",
  "evidence": [
    {
      "sectionId": "database-migration-plan",
      "quote": "The migration will remove deprecated columns after data transformation.",
      "verificationLevel": "quote_verified"
    }
  ]
}
```

## Evaluation

Design Court measures review quality against a seeded benchmark corpus:

```powershell
dotnet run --project src/DesignCourt.Cli -- eval
```

This runs the review workflow over every case in `samples/benchmark.json`, compares reportable findings to the expected findings, and writes `design-court-output/eval-report.md` and `design-court-output/eval-metrics.json`.

Current expected output on the seeded corpus:

```text
Evaluated 2 benchmark case(s).
Precision: 1.00
Recall: 1.00
F1: 1.00
False-positive rate: 0.00
```

See [docs/evaluation.md](docs/evaluation.md) for the manifest format, matching rule, and metric definitions.

## Design Principles

Design Court is built around a few strict rules:

- **Core first**: domain contracts live independently from providers, GitHub APIs, and agent frameworks.
- **Evidence before judgment**: findings are not accepted unless evidence can be located.
- **Judge mandatory**: agents raise candidate findings; only the Judge emits final findings.
- **No fake precision**: early versions use confidence bands, not numeric confidence.
- **Benchmarks matter**: seeded defects and clean controls are part of the product.

## Architecture

```text
src/
  DesignCourt.Core/        Domain models, contracts, invariants
  DesignCourt.Parsing/     Markdown sections and evidence verification
  DesignCourt.Agents/      Deterministic agents, Judge, local workflow
  DesignCourt.Reporting/   Markdown report rendering
  DesignCourt.Cli/         Local command entrypoint

tests/
  DesignCourt.Core.Tests/
  DesignCourt.Parsing.Tests/
  DesignCourt.Agents.Tests/

samples/
  rfcs/
  clean-controls/
  expected-findings/
```

Dependency direction:

```text
DesignCourt.Core
  <- DesignCourt.Parsing
  <- DesignCourt.Reporting
  <- DesignCourt.Agents
  <- DesignCourt.Cli
```

Future Microsoft Agent Framework integration belongs behind infrastructure adapters. It must not leak into `DesignCourt.Core`.

## Testing and Review

Run the full verification suite:

```powershell
dotnet build DesignCourt.slnx
dotnet test DesignCourt.slnx
dotnet run --project src/DesignCourt.Cli -- review samples/rfcs/payment-rfc-missing-rollback.md
```

Then inspect:

```powershell
Get-Content design-court-output/report.md
Get-Content design-court-output/findings.json
```

See [docs/testing.md](docs/testing.md) for the functional scenario, clean-control expectation, and code review checklist.

See [docs/releases.md](docs/releases.md) for versioning, tagging, and release verification.

## Roadmap

### v0.1 - Local Review MVP

- Operations Engineer Agent.
- Evidence-gated Judge.
- Markdown and JSON reports.
- `design-court eval`.
- Precision, recall, F1, and false-positive rate.

### v0.2 - Multi-Agent MVP

- Security Attacker Agent.
- Ambiguity Reviewer Agent.
- Deduplication and finding budget.
- Expanded benchmark corpus.

### v0.3 - GitHub Action

- Pull request comments.
- Changed-files-only review.
- Artifact upload.
- Fail-on severity.

### Later

- Review profiles.
- Microsoft Agent Framework adapter.
- Provider abstraction.
- Debate mode.
- Documentation site.

## Repository Philosophy

This repository is intentionally being built in small, reviewable slices. Each slice should leave behind either functional tests or professional documentation explaining how to verify the behavior.

## License

Apache-2.0. See [LICENSE](LICENSE).
