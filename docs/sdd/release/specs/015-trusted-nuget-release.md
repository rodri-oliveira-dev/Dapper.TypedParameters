# 015 - Trusted NuGet release workflow

## Status

Completed.

## Context

The repository is preparing a future NuGet.org publication for
`TypedParameters.Dapper.SqlServer`. Prompts 013 and 014 finalized the package
identity and proved that the produced `.nupkg` can be consumed by external
`net8.0` and `net10.0` applications.

This prompt creates release automation only. It must not publish a package,
create a tag, push commits, or store a permanent NuGet API key.

## Authoritative configuration

| Item | Value |
| --- | --- |
| NuGet username | `rodri-oliveira-dev` |
| NuGet ownership | Individual |
| GitHub repository owner | `rodri-oliveira-dev` |
| GitHub repository | `Dapper.TypedParameters` |
| Workflow file | `release.yml` |
| GitHub environment | `nuget-release` |
| Package ID | `TypedParameters.Dapper.SqlServer` |

## External documentation checked

Official documentation checked on 2026-08-10:

- NuGet Trusted Publishing:
  <https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing>
- `dotnet nuget push`:
  <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push>
- NuGet symbol packages:
  <https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg>
- GitHub OIDC permissions:
  <https://docs.github.com/en/actions/reference/security/oidc>
- GitHub deployment environments:
  <https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments>
- GitHub `workflow_dispatch` inputs:
  <https://docs.github.com/en/enterprise-cloud@latest/actions/reference/workflows-and-actions/workflow-syntax>
- NuGet Login action:
  <https://github.com/marketplace/actions/nuget-login>

Current operational findings:

- Recommended NuGet action: `NuGet/login@v1`; GitHub Marketplace currently
  shows latest `v1.2.0` under the `v1` major tag.
- Required OIDC permission: the publishing job must grant `id-token: write` so
  GitHub can issue an OIDC JWT. `contents: read` is also required for normal
  repository/artifact operations in this workflow.
- Temporary credential: `NuGet/login@v1` exchanges the GitHub OIDC token with
  NuGet.org and returns the short-lived API key as
  `steps.login.outputs.NUGET_API_KEY`.
- Package push: `dotnet nuget push <package>.nupkg --api-key <temporary-key>
  --source https://api.nuget.org/v3/index.json`.
- Symbol package behavior: the project creates `.snupkg` by setting
  `IncludeSymbols=true` and `SymbolPackageFormat=snupkg`. NuGet.org accepts the
  new `.snupkg` symbol format. `dotnet nuget push` supports symbol behavior for
  packages unless disabled with `--no-symbols`; the workflow does not pass
  `--no-symbols` and uploads `.snupkg` for audit before publish.
- Duplicate publication behavior: the official publish step intentionally does
  not use `--skip-duplicate`; an already published version must fail visibly.

## Release strategy

Release runs are manual through `workflow_dispatch`.

Inputs:

- `package_version`: explicit NuGet version to validate and optionally publish.
- `publish`: boolean, default `false`.

Tag policy:

```text
tag = v<package_version>
```

Examples:

```text
package_version = 0.1.0-preview.1
required tag = v0.1.0-preview.1
```

Publishing fails before OIDC login if the workflow ref is not the expected tag.
This prevents publication from an arbitrary branch. Rehearsal runs may execute
from any ref because they do not request NuGet credentials and do not publish.

## Workflow design

Created:

```text
.github/workflows/release.yml
```

The workflow is separate from CI and uses three jobs:

1. `validate`
   - runs for `net8.0` and `net10.0`;
   - restores;
   - builds;
   - runs unit tests;
   - runs integration tests.
2. `package`
   - depends on `validate`;
   - restores and builds;
   - packs exactly `TypedParameters.Dapper.SqlServer.<version>.nupkg`;
   - runs SDK package compatibility validation;
   - verifies the expected `.nupkg` and `.snupkg` exist;
   - runs package content validation;
   - runs real package consumption validation;
   - uploads the exact validated `.nupkg` and `.snupkg`.
3. `publish`
   - depends on `package`;
   - runs only when `publish == true`;
   - uses `environment: nuget-release`;
   - grants `contents: read` and `id-token: write`;
   - downloads the already validated artifacts;
   - checks `github.ref == refs/tags/v<package_version>`;
   - requests the temporary NuGet credential with `NuGet/login@v1`;
   - pushes the `.nupkg` without `--skip-duplicate`.

No package is rebuilt between validation and publication.

## Permission policy

Default workflow permission:

```yaml
permissions:
  contents: read
```

Validation and package jobs:

```yaml
permissions:
  contents: read
```

Publish job:

```yaml
permissions:
  contents: read
  id-token: write
```

The workflow does not grant `contents: write`, `packages: write`, or
`actions: write`.

## Rehearsal behavior

A normal run with `publish=false` performs restore, build, test, integration
test, pack, package validation, package content validation, real package
consumption, and artifact upload.

With `publish=false`, no job path contains `NuGet/login` or
`dotnet nuget push` because the `publish` job is skipped before its steps run.
No OIDC token or NuGet temporary API key is requested.

## External setup

Created:

```text
docs/release/trusted-publishing.md
```

Updated:

```text
docs/sdd/release/EXTERNAL-SETUP.md
```

The external setup remains pending. The repository owner must configure the
GitHub environment and NuGet Trusted Publishing policy after this commit and
before Prompt 16.

## Implementation results

- Added protected release workflow with Trusted Publishing support.
- Kept release automation separate from `.github/workflows/ci.yml`.
- Kept CI non-publishing.
- Added manual release configuration documentation.
- Updated release SDD handoff and decisions.
- No NuGet API key secret was added or documented for normal release flow.
- No package was published.
- No tag was created.
- No push was performed.

## Validation results

- YAML parse with local PyYAML: passed.
- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed; 0 warnings, 0 errors.
- `dotnet test Dapper.TypedParameters.sln --configuration Release --no-build`:
  passed:
  - unit tests `net8.0`: 240 passed, 0 failed, 0 skipped;
  - unit tests `net10.0`: 240 passed, 0 failed, 0 skipped;
  - integration tests `net8.0`: 35 passed, 0 failed, 0 skipped;
  - integration tests `net10.0`: 35 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages`:
  passed; generated:
  - `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.nupkg`;
  - `TypedParameters.Dapper.SqlServer.0.1.0-preview.1.snupkg`.
- `dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true`:
  passed.
- `./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  passed; inspected `net8.0` and `net10.0` package assets and symbol package.
- `./scripts/Test-PackageConsumption.ps1 -PackageDirectory ./artifacts/packages`:
  passed; `net8.0` and `net10.0` consumers built and ran successfully. The
  first `net8.0` restore hit a transient file lock and passed on the script's
  retry.
- `git diff --check`: passed.
- Manual workflow inspection: passed.
- Confirmed that `publish=false` skips the only job containing `NuGet/login`
  and `dotnet nuget push`.

## Handoff

```text
Last completed prompt: 015
Current status: Completed
Last expected commit: ci: add trusted NuGet release workflow
Next prompt: 016-preview-release-readiness
External setup required before Prompt 16: Yes
Package published: No
```

## Planned commit

```text
ci: add trusted NuGet release workflow
```
