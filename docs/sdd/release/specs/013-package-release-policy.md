# 013 - Package identity and release support policy

## Status

Completed.

## Context

The repository is preparing the first public NuGet preview for a .NET library
that provides explicitly typed SQL Server parameters for Dapper. Earlier prompts
already added the API surface, documentation, SourceLink, symbols, package
validation, public API analyzers, CI validation, integration tests, and local
package inspection.

Prompt 013 defines the public package identity and release support policy before
publication. It must not publish a package, create a release, create a tag, push
commits, or repeat work completed by prompt 011.

## Problem

The existing project metadata still uses the repository and assembly-oriented
identifier `Dapper.TypedParameters.SqlServer` as the NuGet Package ID. The owner
has made an authoritative decision that the public NuGet identity must be
`TypedParameters.Dapper.SqlServer`, while the assembly and namespace remain
`Dapper.TypedParameters.SqlServer`.

The release documentation also needs a formal driver, SQL Server, Azure SQL,
TFM, and versioning policy that distinguishes driver compatibility from what
this repository continuously integration-tests.

## Authoritative decisions

- NuGet Package ID: `TypedParameters.Dapper.SqlServer`.
- NuGet owner: `rodri-oliveira-dev`.
- Ownership type: Individual.
- `Microsoft.Data.SqlClient`: `6.1.6`.
- First preview version: `0.1.0-preview.1`.

These decisions are accepted unless an external blocker is proven. No alternate
package name will be chosen automatically.

## Package identity

| Item | Value |
| --- | --- |
| NuGet Package ID | `TypedParameters.Dapper.SqlServer` |
| NuGet owner | `rodri-oliveira-dev` |
| Ownership type | Individual |
| Repository | `rodri-oliveira-dev/Dapper.TypedParameters` |
| Planned preview | `0.1.0-preview.1` |

NuGet identity is intentionally distinct from assembly and namespace identity.
The package name optimizes discoverability in NuGet, while source and binary
identity remain stable for consumers.

## Assembly and namespace

| Item | Value |
| --- | --- |
| AssemblyName | `Dapper.TypedParameters.SqlServer` |
| Assembly file | `Dapper.TypedParameters.SqlServer.dll` |
| RootNamespace | `Dapper.TypedParameters.SqlServer` |
| Public namespace | `Dapper.TypedParameters.SqlServer` |

The repository name, solution name, project names, assembly name, root
namespace, and C# namespaces are not renamed in this prompt.

## Dependencies

| Dependency | Policy |
| --- | --- |
| Dapper | Preserve `2.1.79` |
| Microsoft.Data.SqlClient | Set exactly `6.1.6` |
| System.Data.SqlClient | Not supported |

Dependency versions remain centralized in `Directory.Packages.props`. Project
files must not add `Version=` attributes to `PackageReference`.

## Microsoft.Data.SqlClient 6.1 external support status

Microsoft Learn currently lists `Microsoft.Data.SqlClient` 6.1 as an actively
supported LTS line with latest patch `6.1.6` and end of support on
2028-08-14.

The official 6.1 target platform notes include .NET 8.0+ on Windows, Linux, and
macOS. The same release notes say .NET Standard 2.0 targeting support returned
for library compatibility, but this repository intentionally targets only
`net8.0` and `net10.0`.

No incompatibility was found in the official Microsoft documentation that
blocks this repository from using `Microsoft.Data.SqlClient` `6.1.6` for the
current API. If restore, build, tests, package validation, or vulnerability
checks prove an incompatibility, this prompt must be marked blocked instead of
silently reverting to `7.0.2`.

External sources consulted:

- Microsoft Learn SqlClient driver support lifecycle:
  <https://learn.microsoft.com/en-us/sql/connect/ado-net/sqlclient-driver-support-lifecycle?view=sql-server-ver17>
- Microsoft Learn `Microsoft.Data.SqlClient` release notes:
  <https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver17>
- NuGet registration endpoint:
  <https://api.nuget.org/v3/registration5-gz-semver2/typedparameters.dapper.sqlserver/index.json>
- NuGet flat container endpoint:
  <https://api.nuget.org/v3-flatcontainer/typedparameters.dapper.sqlserver/index.json>
- NuGet search endpoint:
  <https://azuresearch-usnc.nuget.org/query?q=packageid:TypedParameters.Dapper.SqlServer&prerelease=true>

## TFM policy

The first preview offers both assets:

- `net8.0`
- `net10.0`

The public API must remain equivalent between both TFMs. No TFM gets a distinct
API surface. Any future removal of a TFM requires an explicit compatibility
decision.

## SQL Server policy

Declared driver compatibility target:

```text
SQL Server 2016 through SQL Server 2025
```

The same target applies to:

- `net8.0`
- `net10.0`

This is a driver compatibility declaration through `Microsoft.Data.SqlClient`
6.1. It is not a statement that every SQL Server version is continuously tested
by this repository.

Continuously tested by this repository:

```text
SQL Server 2022 via mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04
```

SQL Server 2016, 2017, 2019, and 2025 are not claimed as CI-tested by this
repository in this prompt.

## Azure policy

Based on the official `Microsoft.Data.SqlClient` 6.1 compatibility matrix:

| Azure service | Policy |
| --- | --- |
| Azure SQL Database | Driver-compatible, not integration-tested by this repository |
| Azure SQL Managed Instance | Driver-compatible, not integration-tested by this repository |
| Azure Synapse Analytics | Driver-compatible, not integration-tested by this repository |

No Azure infrastructure is added in this prompt.

## Versioning

The first planned preview remains:

```text
0.1.0-preview.1
```

Versioning policy:

- `0.x` preview releases are for initial public validation.
- `1.0.0` will happen only after API stabilization and real feedback.

No tag or GitHub release is created in this prompt.

## Package ID availability

NuGet.org public API checks for `TypedParameters.Dapper.SqlServer` returned:

- Registration endpoint: HTTP 404.
- Flat container endpoint: HTTP 404.
- Search endpoint for `packageid:TypedParameters.Dapper.SqlServer`: `totalHits`
  equals `0`.

Package ID availability: externally verified as no existing NuGet.org package
found at the time of prompt 013 validation.

## Acceptance criteria

- Release SDD structure exists under `docs/sdd/release/`.
- `docs/sdd/release/README.md` documents phase rules for prompts 13 through 16.
- `docs/sdd/release/STATUS.md` is initialized and later completed.
- `docs/sdd/release/EXTERNAL-SETUP.md` records external setup as pending.
- Package ID changes to `TypedParameters.Dapper.SqlServer`.
- AssemblyName, RootNamespace, public namespace, repository name, solution name,
  and project names remain unchanged.
- `Microsoft.Data.SqlClient` changes to exactly `6.1.6`.
- Dapper remains `2.1.79`.
- Target frameworks remain `net8.0` and `net10.0`.
- Documentation distinguishes driver compatibility from CI-tested SQL Server.
- Azure SQL compatibility is documented without claiming repository integration
  tests.
- Package validator expects the new Package ID.
- Required validation commands pass or blockers are documented.
- Exactly one commit is created with message
  `build: define NuGet package identity`.
- No package is published, no tag is created, no release is created, and no push
  is performed.

## Implementation results

- Package ID changed to `TypedParameters.Dapper.SqlServer`.
- AssemblyName remains `Dapper.TypedParameters.SqlServer`.
- RootNamespace and public namespace remain `Dapper.TypedParameters.SqlServer`.
- Version remains `0.1.0-preview.1`.
- Target frameworks remain `net8.0` and `net10.0`.
- Dapper remains `2.1.79`.
- `Microsoft.Data.SqlClient` changed to `6.1.6`.
- SourceLink remains configured through `Microsoft.SourceLink.GitHub`.
- SDK symbol packages remain configured through `SymbolPackageFormat=snupkg`.
- SDK package validation remains enabled with strict compatible TFM checks.
- Public API analyzers remain configured with shipped and unshipped baselines.
- CI still validates `net8.0` and `net10.0`, unit tests, integration tests,
  coverage artifacts, package creation, package content inspection, and symbol
  package upload.
- Integration tests still use
  `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04`.
- Package contents validation now treats Package ID and AssemblyName as
  separate identities.

## Risks

- NuGet Package ID could become unavailable between validation and publication.
- `Microsoft.Data.SqlClient` 6.1.6 may expose dependency vulnerabilities or
  package validation differences that were not present with 7.0.2.
- Existing local artifacts may contain old package IDs; validation must inspect
  the newly generated package.
- Documentation can accidentally conflate driver compatibility with CI coverage.

## Validation commands

```bash
dotnet --info
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
docker version
docker info
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages
dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
git diff --check
```

## Validation results

- `dotnet --info`: passed. Active SDK: `10.0.302`; installed SDKs include
  `8.0.423`, `10.0.110`, `10.0.204`, and `10.0.302`.
- `dotnet restore Dapper.TypedParameters.sln`: first attempt failed due a
  transient NuGet cache file lock under `microsoft.data.sqlclient/6.1.6`;
  immediate retry passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed; 0 warnings, 0 errors.
- Unit tests `net8.0`: passed; 240 passed, 0 failed, 0 skipped.
- Unit tests `net10.0`: passed; 240 passed, 0 failed, 0 skipped.
- `docker version`: passed; client `29.6.2-rd`, server `29.5.3`.
- `docker info`: passed; Rancher Desktop WSL Distribution, Linux containers,
  8 CPUs, 15.32 GiB memory. Docker reported warnings for no swap limit support
  and non-default seccomp profile.
- Integration tests `net8.0`: passed; 35 passed, 0 failed, 0 skipped.
- Integration tests `net10.0`: passed; 35 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`:
  passed; generated:
  - `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.nupkg`
  - `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.snupkg`
- `./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  passed; inspected package and symbol package with `net8.0` and `net10.0`
  assets and dependencies on Dapper and Microsoft.Data.SqlClient.
- `dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true`:
  passed.
- `dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive`:
  passed; no vulnerable packages reported for the library, unit tests, or
  integration tests.
- `git diff --check`: passed.

## Planned files

- `Directory.Packages.props`
- `README.md`
- `README.pt-BR.md`
- `CHANGELOG.md`
- `docs/getting-started.md`
- `docs/getting-started.pt-BR.md`
- `docs/decisions/0005-package-identity-and-release-policy.md`
- `docs/sdd/phase-2/STATUS.md`
- `docs/sdd/release/README.md`
- `docs/sdd/release/DECISIONS.md`
- `docs/sdd/release/STATUS.md`
- `docs/sdd/release/EXTERNAL-SETUP.md`
- `docs/sdd/release/specs/013-package-release-policy.md`
- `scripts/Test-PackageContents.ps1`
- `src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj`

## Planned commit

```text
build: define NuGet package identity
```
