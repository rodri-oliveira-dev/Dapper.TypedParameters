# 014 - Package consumption validation

## Status

Completed.

## Context

Prompt 013 finalized the public NuGet identity as
`TypedParameters.Dapper.SqlServer`, while the assembly and public namespace stay
as `Dapper.TypedParameters.SqlServer`. Before release automation is trusted, the
repository must prove that the generated `.nupkg` can be consumed by external
applications without `ProjectReference` shortcuts.

The package must be validated for both supported target frameworks:

- `net8.0`
- `net10.0`

## Problem

Existing unit and integration tests use `ProjectReference`. That validates the
source project, compiled output, analyzers, and direct test behavior, but it
does not prove NuGet package consumption. A `ProjectReference` can hide package
problems such as:

- wrong Package ID;
- missing or mismatched `lib/<tfm>` assets;
- missing transitive dependencies;
- broken `.nuspec` dependency groups;
- accidental reliance on local build output;
- divergence between assembly identity and package identity.

The release pipeline therefore needs a separate smoke test that installs the
real `.nupkg` into newly created external consumers.

## Strategy

Add a reproducible PowerShell validation script:

```text
scripts/Test-PackageConsumption.ps1
```

The script will:

1. accept a package directory;
2. find exactly one `.nupkg` for `TypedParameters.Dapper.SqlServer`;
3. read the exact package version from the package file name;
4. create a temporary validation workspace under `artifacts/`;
5. configure an isolated NuGet global packages folder under that workspace;
6. create one console consumer for `net8.0` and one for `net10.0`;
7. install the package by Package ID and exact version;
8. restore using a temporary `NuGet.Config`;
9. build each consumer;
10. run each consumer and fail on any assertion error.

The validation must not use:

- `ProjectReference`;
- direct `Reference` with `HintPath` to `bin/Release`;
- copied DLLs;
- repository source files copied into the consumer.

## NuGet isolation

The generated local package directory is the authoritative source for
`TypedParameters.Dapper.SqlServer`.

The script will use:

- a temporary `NuGet.Config`;
- package source mapping;
- an isolated `NUGET_PACKAGES` directory;
- exact package version installation.

Package source mapping will map:

| Source | Package patterns |
| --- | --- |
| `local-package` | `TypedParameters.Dapper.SqlServer` |
| `nuget.org` | `*`, except that the target package is already mapped to local |

This prevents accidental resolution of `TypedParameters.Dapper.SqlServer` from
NuGet.org if a public package with the same ID exists in the future. Other
dependencies, including `Dapper` and `Microsoft.Data.SqlClient`, may still be
resolved from NuGet.org.

The isolated cache prevents a previously restored copy of the target package in
the user's global NuGet cache from satisfying the restore silently.

## Consumer API smoke test

Each generated consumer will compile and execute C# code using:

```csharp
using Dapper.TypedParameters.SqlServer;
```

The smoke test will exercise representative public APIs without opening a SQL
Server connection:

- string parameter: `SqlParam.VarChar`;
- numeric parameter: `SqlParam.Decimal` and `SqlParam.Int`;
- binary or identifier parameter: `SqlParam.VarBinary` and
  `SqlParam.UniqueIdentifier`;
- temporal parameter: `SqlParam.Date`, `SqlParam.Time`, and
  `SqlParam.DateTime2`;
- materialization into `Microsoft.Data.SqlClient.SqlCommand`;
- table-valued parameter with `DataTable`;
- output API through `AsOutput`, `OutputValue`, and `GetValue<T>()`.

Assertions will fail the process if:

- the assembly cannot load;
- the public namespace or types cannot compile;
- the expected `SqlDbType` is incorrect;
- basic `Size`, `Precision`, or `Scale` metadata is incorrect;
- expected dependencies are missing at restore, build, or runtime.

Behavior that requires a live SQL Server remains covered by the existing
integration test project and is not duplicated here.

## CI integration

The `pack` job in `.github/workflows/ci.yml` will run the package consumption
script after package content validation and before package upload:

```powershell
./scripts/Test-PackageConsumption.ps1 -PackageDirectory artifacts/packages
```

The step must fail the job on any failed consumer and must not use
`continue-on-error`.

## Acceptance criteria

- SDD spec and report for prompt 014 exist before implementation.
- `STATUS.md` marks prompt 014 as in progress during implementation.
- `scripts/Test-PackageConsumption.ps1` validates real `.nupkg` consumption.
- The script requires exactly one `TypedParameters.Dapper.SqlServer` `.nupkg`.
- The script extracts and uses the exact package version.
- Consumers are created under ignored `artifacts/` paths.
- `NUGET_PACKAGES` is isolated under `artifacts/`.
- `NuGet.Config` uses package source mapping to force the target package from
  the local package feed and allow dependencies from NuGet.org.
- `net8.0` consumer builds and runs.
- `net10.0` consumer builds and runs.
- CI pack job runs the consumption validation before uploading artifacts.
- Temporary artifacts are not versioned.
- Required validation commands pass.
- Report records package ID, version, package file, sources, isolation, APIs,
  results, issues, and conclusion.
- Final handoff marks prompt 014 completed and package publication as `No`.
- Exactly one commit is created with message
  `test: validate packaged library consumption`.

## Implementation results

- Added `scripts/Test-PackageConsumption.ps1`.
- The script creates temporary consumers under ignored `artifacts/` paths.
- The script writes a temporary `NuGet.Config` with local and NuGet.org
  sources.
- Package source mapping pins `TypedParameters.Dapper.SqlServer` to the local
  package source.
- `NUGET_PACKAGES` is isolated under the temporary validation workspace.
- The restored target package hash is compared to the generated local `.nupkg`.
- Consumers are generated for `net8.0` and `net10.0`.
- Consumers reference only `TypedParameters.Dapper.SqlServer` by package ID and
  exact version.
- Consumers use `Dapper.TypedParameters.SqlServer` as the public namespace.
- CI pack job runs package consumption validation after package content
  validation and before artifact upload.

## Validation results

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed with 0 warnings and 0 errors.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`:
  passed.
- `./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  passed.
- `./scripts/Test-PackageConsumption.ps1 -PackageDirectory ./artifacts/packages`:
  passed.
- `net8.0 consumer: passed`.
- `net10.0 consumer: passed`.
