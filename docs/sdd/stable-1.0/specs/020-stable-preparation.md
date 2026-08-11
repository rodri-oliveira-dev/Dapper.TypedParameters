# 020 - Stable Preparation

## Context

Prompt 019 prepared the `1.0.0-rc.1` candidate and left the external release
steps pending for human execution.

Prompt 020 validates the published RC directly from NuGet.org before preparing
the stable `1.0.0` package metadata.

## Goal

Prepare:

```text
TypedParameters.Dapper.SqlServer 1.0.0
```

only after the published `1.0.0-rc.1` package has been validated from
NuGet.org and all compatibility gates pass.

## Scope

- Confirm the RC merge, tag, rehearsal, publication, and public consumption
  gates.
- Validate public package consumption for `1.0.0-rc.1` using only NuGet.org.
- Compare the stable public API contract with the RC contract.
- Review open GitHub issues for release blockers.
- If all gates pass, remove the prerelease suffix and prepare `1.0.0`.
- If any required gate fails, record `BLOCKED` and do not prepare stable
  package metadata.

## Constraints

- Do not publish `1.0.0`.
- Do not create a tag.
- Do not push.
- Do not replace public RC validation with local `.nupkg` consumption.
- Do not introduce public API changes after the RC unless the release is
  blocked and a new RC is recommended.
- Do not create package validation suppressions to hide an RC compatibility
  break.

## Required public RC validation

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 1.0.0-rc.1
```

The package source must be only:

```text
https://api.nuget.org/v3/index.json
```

Expected passing results:

- `net8.0 restore: passed`
- `net8.0 build: passed`
- `net8.0 execution: passed`
- `net10.0 restore: passed`
- `net10.0 build: passed`
- `net10.0 execution: passed`

## Stable version

Only after all gates pass:

```xml
<VersionPrefix>1.0.0</VersionPrefix>
```

The prerelease suffix must be removed.

Expected package artifacts:

```text
TypedParameters.Dapper.SqlServer.1.0.0.nupkg
TypedParameters.Dapper.SqlServer.1.0.0.snupkg
```

## Compatibility Baseline

If supported by the current SDK package validation mechanism, stable `1.0.0`
should use:

```xml
<PackageValidationBaselineVersion>1.0.0-rc.1</PackageValidationBaselineVersion>
```

No new suppressions are allowed to hide incompatibility with the RC.

## Current Result

The published `TypedParameters.Dapper.SqlServer 1.0.0-rc.1` package was
available from NuGet.org and passed isolated public package consumption
validation for `net8.0` and `net10.0`.

## Planned Commit

```text
chore: prepare 1.0.0 release
```
