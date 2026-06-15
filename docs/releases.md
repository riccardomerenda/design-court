# Release and Versioning

Design Court uses SemVer.

## Current Version

The current source version is:

```text
0.1.0-alpha.1
```

This is an engineering preview version. It proves the local review skeleton, deterministic Operations Agent, evidence verification, Judge gating, reporting, and functional tests.

Local builds may append Git build metadata to the CLI version, for example:

```text
design-court 0.1.0-alpha.1+fd1fbf6f32e165e3ccdaf314eec5f28f166f9e34
```

## Version Source of Truth

Versions are declared in `Directory.Build.props`:

```xml
<VersionPrefix>0.1.0</VersionPrefix>
<VersionSuffix>alpha.1</VersionSuffix>
```

Release tags should use the same SemVer value prefixed with `v`:

```text
v0.1.0-alpha.1
```

The README version badge reads the latest GitHub Release, including prereleases.

## Release Checklist

Before creating a tag:

```powershell
dotnet build DesignCourt.slnx
dotnet test DesignCourt.slnx
dotnet run --project src/DesignCourt.Cli -- review samples/rfcs/payment-rfc-missing-rollback.md
dotnet run --project src/DesignCourt.Cli -- eval
dotnet run --project src/DesignCourt.Cli -- --version
```

Then verify:

- `design-court-output/report.md` includes the expected accepted finding.
- `design-court-output/findings.json` uses snake_case enum values.
- `design-court eval` reports precision, recall, and F1 of `1.00` and a false-positive rate of `0.00`.
- `docs/testing.md` matches the actual verification flow.
- `README.md` accurately describes implemented and missing features.

## Tagging

Create an annotated tag for release versions:

```powershell
git tag -a v0.1.0-alpha.1 -m "Release v0.1.0-alpha.1"
git push origin main
git push origin v0.1.0-alpha.1
```

Create a prerelease from the pushed tag:

```powershell
gh release create v0.1.0-alpha.1 --prerelease --title "v0.1.0-alpha.1" --notes "Engineering preview release for the local review skeleton."
```

## Automation Roadmap

The repository already includes CI for build, tests, and CLI smoke testing.

Future release automation should:

- create GitHub Releases from version tags;
- attach packaged CLI artifacts;
- publish benchmark metrics with each release;
- fail release jobs when functional tests or evaluation gates fail.
