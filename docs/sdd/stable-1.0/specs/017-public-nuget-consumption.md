# 017 - Public NuGet Consumption

## Context

Prompt 016 concluded that the preview package was ready to be published, but
its package consumption validation used a locally generated `.nupkg` from
`artifacts/packages`.

Prompt 017 starts the `1.0.0` stabilization phase by validating the package that
is already public on NuGet.org:

```text
TypedParameters.Dapper.SqlServer 0.1.0-preview.1
```

## Problem

The repository already proves local package composition and local package
consumption. It does not yet prove that a clean external application can restore
the published package directly from NuGet.org and execute representative public
APIs for both supported target frameworks.

## Local Package vs Published Package

A local package validation proves the repository can produce a consumable
`.nupkg`. A public package validation proves the release chain after publication:

```text
repository
  -> release workflow
  -> NuGet.org
  -> public NuGet restore
  -> clean consumer
```

Prompt 017 must not use any local `.nupkg` as the package under test.

## Expected Public Version

- Package ID: `TypedParameters.Dapper.SqlServer`
- Version: `0.1.0-preview.1`
- Source: `https://api.nuget.org/v3/index.json`

## NuGet.org Source Policy

The temporary consumer `NuGet.Config` must clear configured sources and add only
NuGet.org:

```xml
<clear />
<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
```

No local package source, `ProjectReference`, `HintPath`, `artifacts/packages`,
`bin/Release` DLL, or repository output may be used to represent the package.

## Cache Isolation

The validation must create an isolated workspace under `artifacts/` and set
`NUGET_PACKAGES` to a directory inside that workspace. It must not clear or rely
on the user's global NuGet package cache.

The script must verify that the restored package exists in the isolated cache
at the exact requested version.

## Consumers

The script must dynamically create clean console applications for:

- `net8.0`
- `net10.0`

Both consumers must restore, build, and execute equivalent API usage without
database access.

## APIs Exercised

The consumer program must use:

- `using Dapper.TypedParameters.SqlServer;`
- `SqlParam.VarChar(...)`
- `SqlParam.NVarChar(...)`
- `SqlParam.Int(...)`
- `SqlParam.Decimal(...)`
- `SqlParam.UniqueIdentifier(...)`
- `SqlParam.VarBinary(...)`
- `SqlParam.Date(...)`
- `SqlParam.DateTime2(...)`
- `AsOutput()`
- `AsInputOutput()`
- `SqlParam.TableValued(...)`

It must materialize representative parameters into
`Microsoft.Data.SqlClient.SqlCommand` and verify basic metadata.

## Acceptance Criteria

- NuGet.org public APIs confirm package `0.1.0-preview.1` exists.
- `net8.0 restore: passed`
- `net8.0 build: passed`
- `net8.0 execution: passed`
- `net10.0 restore: passed`
- `net10.0 build: passed`
- `net10.0 execution: passed`
- Repository restore and Release build pass.
- `git diff --check` passes.
- `docs/sdd/stable-1.0/reports/017-public-nuget-consumption.md` records the
  outcome.
- `EXTERNAL-RELEASE.md` marks preview public consumption completed only if
  validation passes.

## Risks

- NuGet.org indexing latency could block validation.
- The installed SDK might not include `net10.0`.
- Transient network failures can affect restore.
- A hidden local source or global package cache could weaken the proof if not
  isolated.

## Commands

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 0.1.0-preview.1
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
git diff --check
```

## Planned Files

- `scripts/Test-PublicPackageConsumption.ps1`
- `docs/sdd/stable-1.0/README.md`
- `docs/sdd/stable-1.0/DECISIONS.md`
- `docs/sdd/stable-1.0/STATUS.md`
- `docs/sdd/stable-1.0/EXTERNAL-RELEASE.md`
- `docs/sdd/stable-1.0/specs/017-public-nuget-consumption.md`
- `docs/sdd/stable-1.0/reports/017-public-nuget-consumption.md`

## Planned Commit

```text
test: validate published NuGet consumption
```
