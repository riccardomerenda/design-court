# Evaluation

Design Court measures review quality against a seeded benchmark corpus.

## Command

```powershell
dotnet run --project src/DesignCourt.Cli -- eval
dotnet run --project src/DesignCourt.Cli -- eval samples/benchmark.json
```

`eval` runs the local review workflow over every benchmark case, compares the
reportable findings against the expected findings, and writes:

```text
design-court-output/
  eval-report.md
  eval-metrics.json
```

`eval-report.md` is the human-readable summary with a per-case breakdown.
`eval-metrics.json` contains the aggregate metrics for machine consumption.

## Benchmark Manifest

The corpus is described by a manifest (default: `samples/benchmark.json`):

```json
{
  "cases": [
    {
      "document": "rfcs/payment-rfc-missing-rollback.md",
      "expected": "expected-findings/payment-rfc-missing-rollback.json"
    },
    {
      "document": "clean-controls/payment-rfc-clean.md",
      "expected": null
    }
  ]
}
```

Paths are resolved relative to the manifest file. A case with `expected` set to
`null` (or omitted) is a clean control: it must produce no reportable findings.

## Expected Findings

Each expected-findings file is an array describing the defects a document should
surface:

```json
[
  {
    "id": "EXP-001",
    "title": "Missing rollback strategy for database migration",
    "category": "operability",
    "severity": "high",
    "sectionId": "database-migration-plan",
    "quote": "The migration will remove deprecated columns after data transformation.",
    "reason": "The RFC describes a destructive schema migration but does not define a rollback or recovery procedure."
  }
]
```

## Matching Rule

An expected finding is matched by a reportable finding when:

- the finding category equals the expected category; and
- the finding cites evidence in the expected `sectionId`.

Matching is one-to-one: each reportable finding can satisfy at most one expected
finding. Only reportable findings (`accepted`, `partially_accepted`,
`needs_human_review`) are considered. A rejected finding that would otherwise
match is treated as a miss, because it never reaches the report.

## Metrics

Counts are aggregated across the whole corpus:

- **True positive**: an expected finding matched by a reportable finding.
- **False positive**: a reportable finding that matches no expected finding.
- **False negative**: an expected finding with no matching reportable finding.

| Metric | Definition |
| --- | --- |
| Precision | `TP / (TP + FP)` (`1.0` when there are no reportable findings) |
| Recall | `TP / (TP + FN)` (`1.0` when there are no expected findings) |
| F1 | harmonic mean of precision and recall (`0` when both are `0`) |
| False-positive rate | fraction of documents that produced at least one false positive |

The false-positive rate is intentionally document-based rather than
prediction-based, so that clean controls directly penalize noisy output.

## Current Expected Result

On the seeded corpus the deterministic Operations Engineer Agent yields:

```text
Precision: 1.00
Recall: 1.00
F1: 1.00
False-positive rate: 0.00
```
