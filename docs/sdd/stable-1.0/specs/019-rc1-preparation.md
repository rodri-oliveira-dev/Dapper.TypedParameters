# 019 - RC1 Preparation

## Context

Prompt 018 approved the public API freeze for the 1.0 candidate on branch
`release/1.0-hardening`. The repository now needs to prepare the first release
candidate version without publishing it.

## Goal

Prepare a reviewable commit for:

```text
1.0.0-rc.1
```

The RC must be feature-complete. No new features may be added by this prompt.

## Scope

- Project version metadata.
- CHANGELOG release sections.
- README release-state wording.
- Stable 1.0 SDD handoff files.
- Public API baseline verification.
- Public preview consumption verification.
- Local RC package validation.
- Release workflow audit for tag and version inputs.

## Constraints

- Do not change `PackageId`, `AssemblyName`, `RootNamespace`, or TFMs.
- Do not change public API.
- Do not publish a package.
- Do not create a tag.
- Do not push.
- Do not open a pull request.
- Keep `EXTERNAL-RELEASE.md` RC fields pending.

## Version

Expected project properties:

```xml
<VersionPrefix>1.0.0</VersionPrefix>
<VersionSuffix>rc.1</VersionSuffix>
```

Expected package artifacts:

```text
TypedParameters.Dapper.SqlServer.1.0.0-rc.1.nupkg
TypedParameters.Dapper.SqlServer.1.0.0-rc.1.snupkg
```

## Compatibility Baseline

Evaluate the published preview:

```text
0.1.0-preview.1
```

Use `PackageValidationBaselineVersion` only if SDK package validation works
against the published package without artificial suppressions and without
masking the intentional pre-RC stabilization work from Prompt 018.

## Validation Plan

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 0.1.0-preview.1
```

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
docker version
docker info
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
./scripts/Test-PackageContents.ps1 -PackageDirectory artifacts/packages
./scripts/Test-PackageConsumption.ps1 -PackageDirectory artifacts/packages
dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
git diff --check
```

## Acceptance Criteria

- Public API freeze remains approved.
- Public API baseline files remain unchanged.
- The generated package and symbol package use exactly version `1.0.0-rc.1`.
- Public preview `0.1.0-preview.1` remains consumable from NuGet.org.
- Local RC package content and consumption validation pass for `net8.0` and
  `net10.0`.
- Package validation, public API validation, vulnerability audit, and
  `git diff --check` pass.
- `release.yml` supports `package_version=1.0.0-rc.1` and
  `tag=v1.0.0-rc.1`.
- `EXTERNAL-RELEASE.md` keeps all RC publication fields pending.

## Planned Commit

```text
chore: prepare 1.0.0-rc.1
```
