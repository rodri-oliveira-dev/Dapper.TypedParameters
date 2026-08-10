# Preview Release Readiness

## Identity

Package identity is ready:

- Package ID: `TypedParameters.Dapper.SqlServer`
- Assembly: `Dapper.TypedParameters.SqlServer`
- Public namespace: `Dapper.TypedParameters.SqlServer`
- NuGet owner: `rodri-oliveira-dev`
- Ownership: Individual

The NuGet package identity intentionally differs from the assembly and
namespace identity.

## Version

Version audited:

```text
0.1.0-preview.1
```

The project composes this from:

- `VersionPrefix`: `0.1.0`
- `VersionSuffix`: `preview.1`

## Frameworks

The package targets:

- `net8.0`
- `net10.0`

Generated package entries confirmed:

- `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`
- `lib/net8.0/Dapper.TypedParameters.SqlServer.xml`
- `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`
- `lib/net10.0/Dapper.TypedParameters.SqlServer.xml`

Generated symbol package entries confirmed:

- `lib/net8.0/Dapper.TypedParameters.SqlServer.pdb`
- `lib/net10.0/Dapper.TypedParameters.SqlServer.pdb`

## Dependencies

Top-level package dependencies confirmed in `Directory.Packages.props`, package
restore output, and `.nuspec`:

- `Dapper`: `2.1.79`
- `Microsoft.Data.SqlClient`: `6.1.6`

The generated `.nuspec` dependency groups for both `net8.0` and `net10.0`
contain:

- `Dapper 2.1.79`
- `Microsoft.Data.SqlClient 6.1.6`

## SQL Server compatibility policy

Declared driver compatibility target:

```text
SQL Server 2016 through SQL Server 2025
```

This is a `Microsoft.Data.SqlClient` driver compatibility statement, not a claim
that every listed SQL Server version is continuously tested by this repository.

## CI-tested SQL Server

The repository currently integration-tests SQL Server through:

```text
mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04
```

Local Docker diagnostics passed:

- Docker client: `29.6.2-rd`
- Docker engine: `29.5.3`
- Runtime OS: Rancher Desktop WSL Distribution, Linux containers

## Azure compatibility statement

Documented Azure compatibility remains driver compatibility through
`Microsoft.Data.SqlClient` 6.1:

- Azure SQL Database: driver-compatible, not integration-tested here.
- Azure SQL Managed Instance: driver-compatible, not integration-tested here.
- Azure Synapse Analytics: driver-compatible, not integration-tested here.

## Unit tests

Final unit test results:

- `net8.0`: 240 passed, 0 failed, 0 skipped.
- `net10.0`: 240 passed, 0 failed, 0 skipped.

Commands:

```bash
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
```

## Integration tests

Final integration test results:

- `net8.0`: 35 passed, 0 failed, 0 skipped.
- `net10.0`: 35 passed, 0 failed, 0 skipped.

Commands:

```bash
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
```

## Package consumption tests

Real package consumption validation passed for external consumers:

- `net8.0 consumer: passed`
- `net10.0 consumer: passed`

The validation used:

- local source: `./artifacts/packages`;
- NuGet.org for transitive dependencies;
- package source mapping that pins `TypedParameters.Dapper.SqlServer` to the
  local package source;
- isolated `NUGET_PACKAGES`;
- exact version `0.1.0-preview.1`;
- SHA-512 comparison between the generated `.nupkg` and restored local package.

## Package validation

Package generation and validation passed:

- `dotnet pack`: passed.
- `./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  passed.
- `dotnet msbuild ... -target:RunPackageValidation`: passed.

Generated artifacts:

- `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.nupkg`
- `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.snupkg`

The `.nupkg` contains 9 entries and the `.snupkg` contains 6 entries.

`.nuspec` inspection confirmed:

- ID: `TypedParameters.Dapper.SqlServer`
- Version: `0.1.0-preview.1`
- Authors: `Rodrigo de Oliveira`
- License: MIT expression
- README: `README.md`
- Repository: `https://github.com/rodri-oliveira-dev/Dapper.TypedParameters`
- Dependency groups for `net8.0` and `net10.0`

## Public API validation

Public API validation passed:

- `PublicAPI.Shipped.txt` is populated and records 46 public symbols.
- `PublicAPI.Unshipped.txt` contains only `#nullable enable`.
- SDK package validation with strict compatible TFM checks passed.
- API is equivalent for `net8.0` and `net10.0`.
- No unexpected public API change was introduced by prompts 13 through 15.
- No baseline was regenerated to hide breaking changes.

## Security audit

Security audit passed.

Command:

```bash
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
```

Result:

- `Dapper.TypedParameters.SqlServer`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.Tests`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.IntegrationTests`: no vulnerable packages.

No relevant unmitigated vulnerability was found.

## NuGet Package ID availability

NuGet.org public APIs were checked during this audit:

- Registration endpoint:
  <https://api.nuget.org/v3/registration5-gz-semver2/typedparameters.dapper.sqlserver/index.json>
  returned HTTP 404.
- Flat container endpoint:
  <https://api.nuget.org/v3-flatcontainer/typedparameters.dapper.sqlserver/index.json>
  returned HTTP 404.
- Search endpoint:
  <https://azuresearch-usnc.nuget.org/query?q=packageid:TypedParameters.Dapper.SqlServer&prerelease=true>
  returned HTTP 200 with `totalHits: 0`.

Package ID status: Available for first publication.

## Trusted Publishing configuration

External setup evidence is recorded in
`docs/sdd/release/EXTERNAL-SETUP.md`:

```text
NuGetOwner: rodri-oliveira-dev
NuGetOwnershipType: Individual
RepositoryOwner: rodri-oliveira-dev
Repository: Dapper.TypedParameters
WorkflowFile: release.yml
EnvironmentName: nuget-release
GitHubEnvironment: COMPLETED
NuGetTrustedPublishingPolicy: COMPLETED
HumanVerification: COMPLETED
```

This is human verification recorded by the repository owner, not an automated
inspection of private account settings.

Trusted Publishing policy values match
`docs/release/trusted-publishing.md`.

## Release workflow audit

Release workflow audit passed for `.github/workflows/release.yml`.

Confirmed:

- Uses `workflow_dispatch`.
- `publish` input defaults to `false`.
- Rehearsal mode runs validation/package jobs without NuGet login or publish.
- Publish job runs only when `inputs.publish` is true.
- Publish requires `github.ref == refs/tags/v<package_version>`.
- Publish uses `environment: nuget-release`.
- `id-token: write` appears only in the publish job.
- `NuGet/login@v1` appears only in the publish job.
- No permanent `NUGET_API_KEY` secret is used.
- Package validation runs before publish.
- Package consumption validation runs before publish.
- `.nupkg` and `.snupkg` artifacts are uploaded before publish.
- The publish job downloads the validated artifacts and does not rebuild the
  package.
- `--skip-duplicate` is not used.
- No unnecessary `contents: write`, `packages: write`, or `actions: write`
  permission is granted.

YAML parsing passed for `ci.yml` and `release.yml`.

GitHub action refs used by `release.yml` were checked with `git ls-remote` and
exist:

- `actions/checkout@v7`
- `actions/setup-dotnet@v6`
- `actions/upload-artifact@v7`
- `actions/download-artifact@v8`

## Documentation

Documentation audit passed.

Confirmed:

- `README.md` links to `README.pt-BR.md`.
- `README.pt-BR.md` links back to `README.md`.
- English docs link to Portuguese counterparts.
- Portuguese docs link to English counterparts.
- `README.md` says: "The package has not been published to NuGet yet."
- `README.pt-BR.md` also states that the package has not been published to
  NuGet.
- Getting started docs describe local package consumption until publication.
- Documentation distinguishes declared driver compatibility from CI-tested SQL
  Server coverage.
- Documentation does not claim the package is already published.

## Warnings

- `docker info` reported local environment warnings:
  - `No swap limit support`
  - `daemon is not using the default seccomp profile`
- Trusted Publishing setup is based on human verification recorded in
  `EXTERNAL-SETUP.md`; private account configuration was not independently
  inspected by automation in this prompt.

## Blockers

None.

## Final recommendation

READY FOR PREVIEW RELEASE

Recommended human release sequence:

1. Review branch.
2. Push branch.
3. Open/merge PR into main.
4. Confirm CI on main.
5. Create tag v0.1.0-preview.1.
6. Run release.yml against that tag in rehearsal mode.
7. Review generated .nupkg and .snupkg artifacts.
8. Run release.yml with publish=true.
9. Verify package on NuGet.org.
10. Verify symbol package.
11. Install package from NuGet.org in clean net8.0 and net10.0 consumers.
