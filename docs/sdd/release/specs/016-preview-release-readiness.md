# 016 - Preview release readiness

## Status

Completed.

## Context

Prompts 013, 014, and 015 prepared the public NuGet identity, package
consumption validation, and Trusted Publishing workflow for the first preview of
`TypedParameters.Dapper.SqlServer`.

Prompt 016 is the final repository audit before human review, merge, tag, and
publication. It must not publish a package, create a tag, open a pull request,
or push commits.

## External setup evidence

The repository owner updated:

```text
docs/sdd/release/EXTERNAL-SETUP.md
```

Required values are recorded as:

```text
GitHubEnvironment: COMPLETED
NuGetTrustedPublishingPolicy: COMPLETED
HumanVerification: COMPLETED
```

This is operational evidence recorded by the owner. It is not an automated
verification of private GitHub or NuGet account configuration.

## Final identity under audit

| Item | Expected value |
| --- | --- |
| Package ID | `TypedParameters.Dapper.SqlServer` |
| Version | `0.1.0-preview.1` |
| Assembly | `Dapper.TypedParameters.SqlServer` |
| Namespace | `Dapper.TypedParameters.SqlServer` |
| NuGet owner | `rodri-oliveira-dev` |
| Ownership | Individual |
| Microsoft.Data.SqlClient | `6.1.6` |
| Dapper | `2.1.79` |
| TFMs | `net8.0`; `net10.0` |

## Audit scope

- Handoff completion for prompts 013, 014, and 015.
- NuGet.org package ID availability at audit time.
- Full local restore, build, unit test, integration test, pack, package
  content, package consumption, package validation, vulnerability, and public
  API checks.
- Release workflow safety and Trusted Publishing configuration.
- Public documentation release-state check.

## Acceptance criteria

- `EXTERNAL-SETUP.md` records all required external setup items as
  `COMPLETED`.
- Package identity and dependency versions match the accepted release
  decisions.
- NuGet.org does not already contain `TypedParameters.Dapper.SqlServer`, or any
  existing package has compatible ownership.
- Unit tests pass for `net8.0` and `net10.0`.
- Integration tests pass for `net8.0` and `net10.0`.
- Generated `.nupkg` contains both `net8.0` and `net10.0` assets.
- Generated `.snupkg` exists and contains symbols for both TFMs.
- `.nuspec` metadata matches the expected Package ID, version, dependencies,
  license, README, and repository.
- Package consumption validation passes for external `net8.0` and `net10.0`
  consumers.
- SDK package validation passes.
- Vulnerability scan has no relevant unmitigated vulnerability.
- Public API baseline remains coherent and strict compatible TFM validation
  passes.
- Release workflow does not request NuGet credentials in rehearsal mode and
  only publishes from the intended tag.
- No package is published, no tag is created, no pull request is opened, and no
  push is performed.

## Validation plan

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
./scripts/Test-PackageConsumption.ps1 -PackageDirectory ./artifacts/packages
dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
git diff --check
```

## Results

Final recommendation:

```text
READY FOR PREVIEW RELEASE
```

Detailed report:

```text
docs/sdd/release/reports/016-preview-release-readiness.md
```

## Handoff

```text
Last completed prompt: 016
Current status: Completed
Release readiness: READY FOR PREVIEW RELEASE
Version: 0.1.0-preview.1
PackageId: TypedParameters.Dapper.SqlServer
Package published: No
Push performed: No
Pull request opened: No
Tag created: No
```

## Planned commit

```text
chore: finalize preview release readiness
```
