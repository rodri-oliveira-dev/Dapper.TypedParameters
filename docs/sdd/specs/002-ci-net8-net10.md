# 002 - CI for net8.0 and net10.0

## Status

Implemented.

## Context

The repository now targets `net8.0` and `net10.0` for both the library and test project. The CI must make that compatibility visible and fail independently for each target framework.

## Current Workflow State

The workflow is `.github/workflows/ci.yml`.

- Triggers are `push`, `pull_request`, and `workflow_dispatch`.
- Permissions are limited to `contents: read`.
- One `build-and-test` job runs on `ubuntu-latest`.
- The job uses `actions/checkout@v6` and `actions/setup-dotnet@v5`.
- The job installs .NET SDK `10.0.302`.
- Restore and build run once for the solution.
- Tests run once without `--framework`.
- `dotnet pack` runs in the same job after tests.
- No package artifact is uploaded.

## Problem

Because tests run without an explicit `--framework`, CI output does not clearly prove that both `net8.0` and `net10.0` passed. Packaging also happens in the same job and the generated `.nupkg` is not retained as a CI artifact.

## Goals

- Validate `net8.0` explicitly.
- Validate `net10.0` explicitly.
- Keep CI on `Release`.
- Restore before build.
- Build with `--no-restore`.
- Test each TFM with `--framework` and `--no-build`.
- Fail the workflow if either TFM fails.
- Pack only after all validation matrix entries pass.
- Upload only the generated `.nupkg` as an artifact.
- Preserve current triggers and minimal permissions.
- Avoid any NuGet publication step or secret dependency.

## Non-goals

- No package publication.
- No push or pull request creation.
- No package version change.
- No source code or test changes.
- No symbol package upload.
- No introduction of CI secrets.
- No change to public API or target frameworks.

## Matrix Design

Use a single validation job with an explicit matrix:

```yaml
framework:
  - net8.0
  - net10.0
```

Each matrix entry restores the solution, builds the solution for its framework, and runs tests for the same framework. `fail-fast: false` keeps both framework results visible when one fails.

## Restore, Build, and Test Strategy

Each validation matrix entry:

1. Checks out the repository.
2. Installs the .NET SDKs needed for the supported TFMs.
3. Runs `dotnet restore Dapper.TypedParameters.sln`.
4. Runs `dotnet build Dapper.TypedParameters.sln --framework <TFM> --configuration Release --no-restore`.
5. Runs `dotnet test Dapper.TypedParameters.sln --framework <TFM> --configuration Release --no-build`.

This keeps restore explicit and prevents build and test from silently repeating previous stages.

## Pack Strategy

Use a separate pack job with `needs: validate`.

The pack job checks out the repository, installs the same SDK set, restores the solution, and runs:

```bash
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-restore --output artifacts/packages
```

The pack command intentionally does not pass `--framework`, so the multi-targeted project produces one package containing both `net8.0` and `net10.0` assets.

## Expected Artifacts

- `Dapper.TypedParameters.SqlServer.<version>.nupkg`

The artifact upload should include only `artifacts/packages/*.nupkg`.

## Acceptance Criteria

- Workflow YAML is valid.
- Push to any branch still triggers CI.
- Pull requests still trigger CI.
- Manual dispatch remains available.
- `net8.0` is tested explicitly.
- `net10.0` is tested explicitly.
- Pack runs only after successful validation.
- The `.nupkg` is uploaded as an artifact.
- No NuGet publish step exists.
- Workflow permissions remain `contents: read`.
- No new secret is required.
- Jobs have reasonable `timeout-minutes`.
- Concurrency cancels older runs for the same branch or pull request ref only.

## Validation

Planned local validation:

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build
dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
git diff --check
git status --short
```

The final YAML will also be inspected manually.

## Risks

- CI requires a runner image and setup action capable of installing the configured .NET SDKs.
- `net10.0` validation depends on SDK `10.0.302`, matching `global.json`.
- Local validation does not prove remote GitHub Actions execution.

## Evidence

- Branch before implementation: `feat/string-parameters`.
- Working tree before implementation: clean.
- Recent history before implementation:
  - `6cfaf44 build: target net8.0 and net10.0`
  - `50abd3f chore: configure repository development baseline`
  - `308c68e Merge pull request #1 from rodri-oliveira-dev/feat/string-parameters`
  - `cae1e20 test: cover SQL Server string parameters`
  - `49a8835 feat: implement SQL Server typed parameters`
- `docs/sdd/STATUS.md` indicated `Last completed prompt: 001`.
- `docs/sdd/STATUS.md` indicated `Next prompt: 002-ci-net8-net10`.
- Library project TFMs before implementation: `net8.0;net10.0`.
- Test project TFMs before implementation: `net8.0;net10.0`.
- Final workflow preserves `push`, `pull_request`, and `workflow_dispatch`.
- Final workflow preserves `permissions: contents: read`.
- Final workflow uses `actions/checkout@v6`, `actions/setup-dotnet@v5`, and `actions/upload-artifact@v4`.
- Final workflow validates `net8.0` and `net10.0` through an explicit matrix.
- Final workflow packs only after the validation job succeeds.
- Final workflow does not publish to NuGet and does not require secrets.

## Validation Results

- `dotnet restore Dapper.TypedParameters.sln`: passed; all projects were up to date for restore.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; produced `net8.0` and `net10.0` outputs for library and tests; 0 warnings, 0 errors.
- `dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build`: passed; 29 tests passed.
- `dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build`: passed; 29 tests passed.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; created `artifacts/packages/Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- `dotnet build Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- `dotnet build Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-restore --output artifacts/packages`: passed; produced the same multi-target package shape used by CI.
- Package inspection confirmed `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`, `lib/net8.0/Dapper.TypedParameters.SqlServer.xml`, `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`, and `lib/net10.0/Dapper.TypedParameters.SqlServer.xml`.
- `git diff --check`: passed; emitted a line-ending notice that `.github/workflows/ci.yml` will be normalized from CRLF to LF the next time Git touches it.
- `git status --short`: showed only files belonging to this task.
- Final YAML was inspected in full locally.

## Limitations

- GitHub Actions remote execution was not run or observed in this task.
- No package was published.
- No push or pull request was created.

## Commit Planned

```text
ci: validate net8.0 and net10.0
```
