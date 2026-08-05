# 001 - Multi-target net8.0 and net10.0

## Status

Implemented.

## Context

The repository contains a public .NET library package named `Dapper.TypedParameters.SqlServer` and a test project. The package is provider-specific for SQL Server through `Microsoft.Data.SqlClient`.

## Problem

The library currently includes a `netstandard2.0` asset and the tests run only on `net8.0`. The requested package shape is a single NuGet package with assets for `net8.0` and `net10.0`.

## Goals

- Target the library to `net8.0` and `net10.0`.
- Target the tests to `net8.0` and `net10.0`.
- Preserve the existing public API of `SqlParam` and `TypedSqlParameter`.
- Preserve Central Package Management and current package versions.
- Create the initial SDD handoff structure.

## Non-goals

- No package publication.
- No push or pull request creation.
- No package version updates.
- No public API changes.
- No provider-neutral `Core` or `Abstractions` package.
- No conditional references by target framework.

## Functional Requirements

- The library must pack as `Dapper.TypedParameters.SqlServer`.
- The package must contain assets for `net8.0` and `net10.0`.
- Existing tests must be preserved and executed for both TFMs when the SDK supports them.

## Technical Requirements

- Use `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` in the library project.
- Use `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` in the test project.
- Preserve `Nullable`, `TreatWarningsAsErrors`, deterministic build, XML documentation, and Central Package Management.
- Preserve Dapper `2.1.79`.
- Preserve `Microsoft.Data.SqlClient` `7.0.2`.
- Do not add TFM-specific conditions unless an incompatibility is proven and documented.

## Proposed Design

Change only project targeting metadata and compatibility documentation. Keep dependencies unconditioned and centrally versioned.

## Planned Files

- `src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj`
- `tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj`
- `README.md`
- `docs/decisions/0002-target-frameworks.md`
- `docs/sdd/README.md`
- `docs/sdd/DECISIONS.md`
- `docs/sdd/STATUS.md`
- `docs/sdd/specs/001-multitarget-net8-net10.md`

## Changed Files

- `README.md`
- `docs/decisions/0002-target-frameworks.md`
- `docs/sdd/README.md`
- `docs/sdd/DECISIONS.md`
- `docs/sdd/STATUS.md`
- `docs/sdd/specs/001-multitarget-net8-net10.md`
- `src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj`
- `tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj`

## Acceptance Criteria

- Library project targets `net8.0;net10.0`.
- Test project targets `net8.0;net10.0`.
- `netstandard2.0` is removed.
- Package references remain unconditioned.
- Package versions remain centralized and unchanged.
- SDD files exist and reflect the completed handoff.
- Exactly one semantic commit is created with message `build: target net8.0 and net10.0`.

## Validation Commands

```bash
dotnet --info
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build --verbosity normal
dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build --verbosity normal
git diff --check
git status --short
```

## Risks

- Local validation requires an installed .NET SDK that supports `net10.0`.
- CI may require follow-up changes to validate each TFM explicitly.

## Evidence Obtained

- Branch before implementation: `feat/string-parameters`.
- Working tree before implementation: clean.
- Recent history before implementation:
  - `50abd3f chore: configure repository development baseline`
  - `308c68e Merge pull request #1 from rodri-oliveira-dev/feat/string-parameters`
  - `cae1e20 test: cover SQL Server string parameters`
  - `49a8835 feat: implement SQL Server typed parameters`
  - `efdd557 feat: define SQL Server parameter API`
- Existing projects: `Dapper.TypedParameters.SqlServer` and `Dapper.TypedParameters.SqlServer.Tests`.
- Current library TFMs: `netstandard2.0;net8.0`.
- Current test TFM: `net8.0`.
- Current Dapper version: `2.1.79`.
- Current `Microsoft.Data.SqlClient` version: `7.0.2`.
- Existing `docs/sdd/`: absent.
- TFM conditionals: none found.
- Code uses `Microsoft.Data.SqlClient`; `System.Data.SqlClient` was not found in source or tests.
- No shared code API incompatible with `net8.0` or `net10.0` was identified.

## Validation Results

- `dotnet --info`: passed. Active SDK: `10.0.302`; installed SDKs include `8.0.423`, `10.0.110`, `10.0.204`, and `10.0.302`.
- `dotnet restore Dapper.TypedParameters.sln`: passed; all projects were up to date for restore.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; produced `net8.0` and `net10.0` outputs for library and tests; 0 warnings, 0 errors.
- `dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build --verbosity normal`: passed; 29 tests passed.
- `dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build --verbosity normal`: passed; 29 tests passed.
- `git diff --check`: passed; emitted line-ending notices that `README.md`, `src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj`, and `tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj` will be normalized from CRLF to LF the next time Git touches them.
- `git status --short`: showed only files belonging to this task.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages`: passed; created `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- Package content inspection: confirmed `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`, `lib/net8.0/Dapper.TypedParameters.SqlServer.xml`, `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`, and `lib/net10.0/Dapper.TypedParameters.SqlServer.xml`.

## Limitations

- No package was published.
- No push or pull request was created.
- CI workflow behavior is left for the next prompt.

## Commit Message

```text
build: target net8.0 and net10.0
```
