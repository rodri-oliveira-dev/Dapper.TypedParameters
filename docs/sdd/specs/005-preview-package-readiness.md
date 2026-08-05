# 005 - Preview package readiness

## Status

Completed.

## Scope

Validate the local NuGet preview package `Dapper.TypedParameters.SqlServer` version `0.1.0-preview.1` before human pull request review. The validation covers metadata, multi-target package assets, dependency shape, package contents, test results, public API equivalence between `net8.0` and `net10.0`, and handoff readiness.

## Expected Metadata

- Package ID: `Dapper.TypedParameters.SqlServer`.
- Version: `0.1.0-preview.1`, composed by `VersionPrefix` `0.1.0` and `VersionSuffix` `preview.1`.
- Title: `Dapper Typed Parameters for SQL Server`.
- Description: `Explicit SQL Server parameter types for Dapper using Microsoft.Data.SqlClient.`
- Authors: `Rodrigo de Oliveira`.
- License: MIT through `PackageLicenseExpression`.
- README: package root `README.md` through `PackageReadmeFile`.
- Repository URL: `https://github.com/rodri-oliveira-dev/Dapper.TypedParameters`.
- Repository type: `git`.
- Tags: `dapper;sql-server;sqlclient;ado.net;parameters;micro-orm`.
- Deterministic build: enabled.
- Symbols: no symbol package configuration is expected unless already configured.
- Dependencies: unconditioned references to Dapper and Microsoft.Data.SqlClient using Central Package Management.

## Expected Package Contents

The generated `.nupkg` must contain:

- `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`.
- `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`.
- `README.md`.
- A `.nuspec` file with correct metadata and dependencies.
- XML documentation files for both TFMs.
- No test DLLs.
- No `bin/` or `obj/` folders.
- No secrets.
- No unintended source files.
- No duplicate artifacts.

## Validation Strategy

1. Confirm branch, clean working tree, and prompt sequence.
2. Read the SDD sources, ADRs, package metadata, workflow, README, and changelog.
3. Run restore, build, unit tests, integration tests, pack, and whitespace validation.
4. Treat the `.nupkg` as a ZIP and inspect entries.
5. Extract and inspect the `.nuspec`.
6. Compare public API exposed by `net8.0` and `net10.0` assemblies using a local inspection script that does not add permanent dependencies.
7. Remove generated package artifacts before commit and confirm `artifacts/` is ignored or absent from the commit.
8. Fill the readiness report with real evidence and PR preparation text.

## Acceptance Criteria

- Build passes for both TFMs.
- Unit tests pass for `net8.0` and `net10.0`.
- Integration tests pass for `net8.0` and `net10.0`, or an environmental blocker is documented.
- The package is generated locally.
- The package contains `net8.0` and `net10.0` assets.
- README is included in the package.
- Metadata is consistent with the expected preview.
- Public API is equivalent between TFMs.
- No binary artifacts are versioned.
- Readiness report is complete.
- `STATUS.md` ends with prompt 005 completed or blocked.
- Exactly one commit is created with message `chore: prepare preview package`.

## Non-goals

- No new library functionality.
- No public API changes.
- No architectural refactoring.
- No package rename.
- No NuGet publication.
- No push, tag, release, or pull request creation.
- No second versioning mechanism.
- No permanent inspection dependency added only for this validation.

## Risks

- Integration tests depend on Docker and SQL Server container startup.
- Package ID values beginning with `Dapper` may be subject to NuGet reserved prefix rules.
- Local validation does not prove remote GitHub Actions execution.
- SDK availability is required for both `net8.0` and `net10.0`.
- Package inspection can miss semantic issues that only appear after NuGet publication or consumer installation.

## Commands

```bash
dotnet --info
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
git diff --check
```

## Evidence

- Branch before validation: `feat/string-parameters`.
- Working tree before validation: clean.
- Recent commits before validation:
  - `b1b91e1 docs: document usage and compatibility`
  - `7358639 test: add Dapper SQL Server integration coverage`
  - `e4c7f83 ci: validate net8.0 and net10.0`
  - `6cfaf44 build: target net8.0 and net10.0`
  - `50abd3f chore: configure repository development baseline`
- `docs/sdd/STATUS.md` before validation indicated `Last completed prompt: 004` and `Next prompt: 005-preview-package-readiness`.
- `dotnet --info`: passed. Active SDK `10.0.302`; installed SDKs include `8.0.423`, `10.0.110`, `10.0.204`, and `10.0.302`.
- `dotnet restore Dapper.TypedParameters.sln`: passed; all projects were up to date for restore.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- Unit tests `net8.0`: passed; 29 passed, 0 failed, 0 skipped.
- Unit tests `net10.0`: passed; 29 passed, 0 failed, 0 skipped.
- Integration tests `net8.0`: passed; 8 passed, 0 failed, 0 skipped.
- Integration tests `net10.0`: passed; 8 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; created `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- `git diff --check`: passed.
- Package inspection confirmed `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`, `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`, `README.md`, XML documentation files, and `Dapper.TypedParameters.SqlServer.nuspec`.
- Package inspection found no test DLLs, no `bin/` or `obj/` paths, no source files, no secret-like file names, and no duplicate file paths.
- `.nuspec` confirmed package ID `Dapper.TypedParameters.SqlServer`, version `0.1.0-preview.1`, MIT license expression, package README, repository URL, and dependency groups for `net8.0` and `net10.0`.
- Public API inspection found no differences between `net8.0` and `net10.0`.
- Full validation results are recorded in `docs/sdd/reports/005-preview-package-readiness.md`.
