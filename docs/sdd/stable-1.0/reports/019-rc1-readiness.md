# 019 - RC1 Readiness

## Identity

- Package ID: `TypedParameters.Dapper.SqlServer`
- Assembly: `Dapper.TypedParameters.SqlServer`
- Public namespace: `Dapper.TypedParameters.SqlServer`
- NuGet owner: `rodri-oliveira-dev`
- Ownership: Individual

No identity fields were changed for the RC preparation.

## Version

Project version:

- `VersionPrefix`: `1.0.0`
- `VersionSuffix`: `rc.1`

Generated artifacts:

- `TypedParameters.Dapper.SqlServer.1.0.0-rc.1.nupkg`
- `TypedParameters.Dapper.SqlServer.1.0.0-rc.1.snupkg`

Package published by this prompt: No.

## Public API freeze

Public API freeze remains:

```text
APPROVED
```

`PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` were not changed by this
prompt. `PublicAPI.Unshipped.txt` still contains only `#nullable enable`.

Public API validation passed through Release build with public API analyzers and
SDK package validation.

## TFMs

Target frameworks remain unchanged:

- `net8.0`
- `net10.0`

Package content validation confirmed `lib/net8.0` and `lib/net10.0` DLL/XML
assets and symbol package PDBs.

## Dependencies

Dependencies remain centrally versioned:

- `Dapper`: `2.1.79`
- `Microsoft.Data.SqlClient`: `6.1.6`

No `PackageReference Version=` attributes were added.

## Unit tests

- `net8.0`: passed, 242 tests.
- `net10.0`: passed, 242 tests.

Commands:

```bash
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
```

## Integration tests

Docker diagnostics passed:

- Client: `29.6.2-rd`
- Server: `29.5.3`

Integration results:

- `net8.0`: passed, 35 tests.
- `net10.0`: passed, 35 tests.

Commands:

```bash
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
```

## Local package consumption

Local package consumption passed for the RC package.

Command:

```powershell
./scripts/Test-PackageConsumption.ps1 -PackageDirectory artifacts/packages
```

Results:

- `net8.0 consumer: passed`
- `net10.0 consumer: passed`

## Public preview consumption

The published preview baseline remains publicly consumable from NuGet.org.

Command:

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 0.1.0-preview.1
```

Results:

- `net8.0 restore: passed`
- `net8.0 build: passed`
- `net8.0 execution: passed`
- `net10.0 restore: passed`
- `net10.0 build: passed`
- `net10.0 execution: passed`

## Package validation

Package validation passed.

Commands:

```bash
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
./scripts/Test-PackageContents.ps1 -PackageDirectory artifacts/packages
dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true
```

Package contents summary:

- `.nupkg`: 9 entries.
- `.snupkg`: 6 entries.
- Frameworks: `net8.0`, `net10.0`.
- Dependencies: `Dapper`, `Microsoft.Data.SqlClient`.

## Compatibility baseline

`PackageValidationBaselineVersion` was set to:

```text
0.1.0-preview.1
```

The SDK package validation mechanism is compatible with the current project and
passed against the published preview without suppressions.

The Prompt 018 scalar metadata reset is an intentional pre-1.0 behavioral
stabilization. It is documented in the changelog and SDD decisions; no
suppression was added to hide a compatibility failure.

## Security

Vulnerability audit passed.

Command:

```bash
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
```

Result:

- `Dapper.TypedParameters.SqlServer`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.Tests`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.IntegrationTests`: no vulnerable packages.

## Documentation

Documentation was updated to state:

- Current stable: none yet.
- Current release candidate: none published yet.
- Upcoming release candidate: `1.0.0-rc.1`.
- Current public preview: `0.1.0-preview.1`.

`CHANGELOG.md` now has the expected sections:

- `Unreleased`
- `1.0.0-rc.1`
- `0.1.0-preview.1`

The RC is explicitly described as a release candidate, not stable.

## Release workflow

`.github/workflows/release.yml` was audited and not changed.

Confirmed:

- `package_version=1.0.0-rc.1` matches the workflow version regex.
- Publish tag resolves to `v1.0.0-rc.1`.
- Publish still requires `github.ref == refs/tags/v<package_version>`.
- `publish=false` remains rehearsal mode.
- `NuGet/login@v1` and `dotnet nuget push` remain only in the publish job.

## Warnings

- `docker info` reported local environment warnings:
  - `No swap limit support`
  - `daemon is not using the default seccomp profile`
- Existing preview package artifacts were archived under ignored
  `artifacts/packages/preview-baseline/` so the package directory root contained
  only the RC `.nupkg` and `.snupkg` during validation.

## Blockers

None.

## Final recommendation

READY FOR 1.0.0-RC.1
